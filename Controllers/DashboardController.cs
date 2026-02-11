using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats/{userId}")]
        public async Task<IActionResult> GetStats(int userId)
        {
            var logs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            // Beregn total point
            int totalPoints = logs.Sum(l => l.PointsEarned);

            // Beregn Streak
            int streak = 0;
            var today = DateTime.Today;

            // Vi tjekker baglæns fra i går
            foreach (var log in logs)
            {
                if (log.TasksCompleted >= log.DailyGoal)
                {
                    streak++;
                }
                else
                {
                    break; // Streak brudt
                }
            }

            // Tjek dagens status (ikke i log endnu)
            int doneToday = await _context.DynamicTasks.CountAsync(t => t.UserId == userId && t.IsCompleted)
                          + await _context.StaticTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);

            return Ok(new
            {
                Streak = streak,
                TotalPoints = totalPoints,
                TodayCompleted = doneToday,
                DailyGoal = 3,
                RecentLogs = logs.Take(7) // Til en lille graf
            });
        }
    }
}