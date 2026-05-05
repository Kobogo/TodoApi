using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;

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
                // 1. Hent alle mulige achievements i systemet
                var allAchievements = await _context.Achievements.ToListAsync();

                // 2. Hent ID'erne på de achievements brugeren allerede har låst op
                var unlockedIds = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .Select(ua => ua.AchievementId)
                    .ToListAsync();

                // 3. Beregn den aktuelle "Task" tæller (summen af alle udførte opgaver fra loggen)
                var currentTaskCount = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .SumAsync(l => (int?)l.TasksCompleted) ?? 0;

                // 4. Hent brugerens aktuelle point/opsparing
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.TotalPoints })
                    .FirstOrDefaultAsync();

                var currentSavings = user?.TotalPoints ?? 0;

                // Returnér det samlede objekt til frontenden
                return Ok(new {
                    AllAchievements = allAchievements,
                    UnlockedIds = unlockedIds,
                    CurrentTaskCount = currentTaskCount,
                    CurrentSavings = currentSavings
                });
            }
            catch (Exception ex)
            {
                // Log fejlen internt hvis nødvendigt
                return StatusCode(500, "Der opstod en fejl ved hentning af achievements.");
            }
        }
    }
}