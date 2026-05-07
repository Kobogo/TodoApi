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

            // 1. Hent ID'er på de achievements brugeren ALLEREDE har
            var earnedIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            // 2. Find alle achievements som brugeren IKKE har endnu
            var potentialAchievements = await _context.Achievements
                .Where(a => !earnedIds.Contains(a.Id))
                .ToListAsync();

            int totalTasks = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .SumAsync(l => (int?)l.TasksCompleted) ?? 0;

            int totalPoints = user.TotalPoints;

            bool anyNewAchievements = false;

            foreach (var achievement in potentialAchievements)
            {
                int currentValue = achievement.Category == "Tasks" ? totalTasks : totalPoints;

                if (currentValue >= achievement.RequirementValue)
                {
                    // VI OPRETTER KUN RÆKKEN HER - Vi giver IKKE emeralds endnu!
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.Id,
                        UnlockedAt = DateTime.UtcNow,
                        IsRewardClaimed = false // Sørg for at denne er false
                    });

                    anyNewAchievements = true;
                }
            }

            if (anyNewAchievements)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}