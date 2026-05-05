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

        public async Task CheckAndAwardAchievementsAsync(int userId, string category)
        {
            // 1. Hent brugeren
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // 2. Find de achievements i kategorien, som brugeren IKKE har endnu
            var unlockedAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var potentialAchievements = await _context.Achievements
                .Where(a => a.Category == category && !unlockedAchievementIds.Contains(a.Id))
                .ToListAsync();

            if (!potentialAchievements.Any()) return;

            // 3. Hent den aktuelle værdi baseret på kategorien
            int currentValue = 0;
            if (category == "Tasks")
            {
                currentValue = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .SumAsync(l => l.TasksCompleted);
            }
            else if (category == "TotalPoints")
            {
                currentValue = user.TotalPoints;
            }

            // 4. Tjek hver potentiel achievement
            foreach (var achievement in potentialAchievements)
            {
                if (currentValue >= achievement.RequirementValue)
                {
                    // Lås op!
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.Id,
                        UnlockedAt = DateTime.UtcNow
                    });

                    // Giv belønning (Achievement Points / Valuta)
                    user.TotalPoints += achievement.RewardAchievementPoints;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}