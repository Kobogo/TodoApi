using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AchievementController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAchievements(int userId)
        {
            try
            {
                var allAchievements = await _context.Achievements.ToListAsync();

                // 2. Hent både ID og IsRewardClaimed status
                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToListAsync();

                var unlockedIds = userAchievements.Select(ua => ua.AchievementId).ToList();

                // 3. Find de ID'er, hvor belønningen ALLEREDE er hentet
                var claimedIds = userAchievements
                    .Where(ua => ua.IsRewardClaimed)
                    .Select(ua => ua.AchievementId)
                    .ToList();

                var currentTaskCount = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .SumAsync(l => (int?)l.TasksCompleted) ?? 0;

                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.TotalPoints, u.Emeralds }) // Vi tager også Emeralds med nu
                    .FirstOrDefaultAsync();

                return Ok(new {
                    AllAchievements = allAchievements,
                    UnlockedIds = unlockedIds,
                    ClaimedIds = claimedIds, // VIGTIG: Nu ved frontenden hvad der er hentet!
                    CurrentTaskCount = currentTaskCount,
                    CurrentSavings = user?.TotalPoints ?? 0,
                    Emeralds = user?.Emeralds ?? 0
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Der opstod en fejl ved hentning af achievements.");
            }
        }

        [HttpPost("claim/{achievementId}")]
        public async Task<IActionResult> ClaimReward(int achievementId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Hent ID fra Token

            // 1. Find koblingen mellem bruger og achievement
            var userAchievement = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (userAchievement == null)
                return BadRequest("Du har ikke optjent dette trofæ endnu.");

            if (userAchievement.IsRewardClaimed)
                return BadRequest("Belønningen er allerede hentet.");

            // 2. Hent selve achievement for at se reward-værdien
            var achievement = await _context.Achievements.FindAsync(achievementId);
            var user = await _context.Users.FindAsync(userId);

            if (achievement == null || user == null) return NotFound();

            // 3. Opdater data
            userAchievement.IsRewardClaimed = true;
            user.Emeralds += achievement.RewardAchievementPoints;

            await _context.SaveChangesAsync();

            return Ok(new { newEmeraldCount = user.Emeralds });
        }
    }
}