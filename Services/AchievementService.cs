using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly AppDbContext _context;

        public AchievementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CheckAndAwardAchievementsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // 1. Hent alle achievements som brugeren IKKE har endnu (uanset kategori)
            var earnedIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var potentialAchievements = await _context.Achievements
                .Where(a => !earnedIds.Contains(a.Id))
                .ToListAsync();

            // 2. Cache tallene så vi ikke henter dem i et loop
            int totalTasks = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .SumAsync(l => (int?)l.TasksCompleted) ?? 0;

            int totalPoints = user.TotalPoints;

            // 3. Evaluer hver potentiel achievement
            foreach (var achievement in potentialAchievements)
            {
                int currentValue = achievement.Category == "Tasks" ? totalTasks : totalPoints;

                if (currentValue >= achievement.RequirementValue)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.Id,
                        UnlockedAt = DateTime.UtcNow
                    });

                    // Her bruger vi RewardAchievementPoints som mængden af emeralder
                    user.Emeralds += achievement.RewardAchievementPoints;
                    // Opdater også en samlet "AchievementPoints" hvis vi vil skelne mellem points optjent via opgaver og points optjent via achievements
                    user.AchievementPoints += achievement.RewardAchievementPoints;
                    // Giv point for de "gamle" achievements de nu har fortjent
                    user.TotalPoints += achievement.RewardAchievementPoints;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}