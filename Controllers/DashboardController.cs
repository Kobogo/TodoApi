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
            // Hent historiske logs
            var logs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            // 1. Beregn basale point fra historikken
            int totalPointsFromHistory = logs.Sum(l => l.PointsEarned);

            // 2. Beregn Streak (start med historikken)
            int streak = 0;
            foreach (var log in logs)
            {
                if (log.TasksCompleted >= log.DailyGoal) streak++;
                else break;
            }

            // 3. Tjek dagens status (vigtigt for "Live" følelsen)
            int doneToday = await _context.DynamicTasks.CountAsync(t => t.UserId == userId && t.IsCompleted)
                          + await _context.StaticTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);

            int dailyGoal = 3; // Standardmål
            int pointsToday = doneToday * 10; // 10 point pr. opgave i dag

            // 4. Hvis målet er nået i dag, tæller vi den med i streaken med det samme!
            if (doneToday >= dailyGoal)
            {
                streak++;
            }

            // Samlet point (Historik + hvad der er optjent indtil videre i dag)
            int liveTotalPoints = totalPointsFromHistory + pointsToday;

            return Ok(new
            {
                Streak = streak,
                TotalPoints = liveTotalPoints,
                TodayCompleted = doneToday,
                DailyGoal = dailyGoal,
                RecentLogs = logs.Take(7) // Historik til listen i frontend
            });
        }
    }
}