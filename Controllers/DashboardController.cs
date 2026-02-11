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
            var todayDate = DateTime.Today;

            // Hent historiske logs
            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            var historyLogs = allLogs.Where(l => l.Date < todayDate).ToList();
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayDate);

            // 1. Beregn basale point fra historikken
            int totalPointsFromHistory = historyLogs.Sum(l => l.PointsEarned);

            // 2. Tjek dagens status (VIGTIG RETTELSE HER)
            // Vi tæller dynamiske opgaver for brugeren
            int dynamicDone = await _context.DynamicTasks
                .CountAsync(t => t.UserId == userId && t.IsCompleted);

            // Vi tæller statiske opgaver (både brugerens egne og de fælles opgaver med UserId == null)
            int staticDone = await _context.StaticTasks
                .CountAsync(t => (t.UserId == userId || t.UserId == null) && t.IsCompleted);

            int activeDoneToday = dynamicDone + staticDone;

            // Brug loggen hvis den er højere (pga. slettede opgaver)
            int effectiveDoneToday = todayLog != null ? Math.Max(todayLog.TasksCompleted, activeDoneToday) : activeDoneToday;

            int dailyGoal = 3;
            int pointsToday = effectiveDoneToday * 10;

            // 3. Beregn Streak
            int streak = 0;
            if (effectiveDoneToday >= dailyGoal)
            {
                streak++;
            }

            foreach (var log in historyLogs)
            {
                if (log.TasksCompleted >= log.DailyGoal) streak++;
                else break;
            }

            // 4. Samlet point
            int liveTotalPoints = totalPointsFromHistory + pointsToday;

            return Ok(new
            {
                Streak = streak,
                TotalPoints = liveTotalPoints,
                TodayCompleted = effectiveDoneToday,
                DailyGoal = dailyGoal,
                RecentLogs = historyLogs.Take(7)
            });
        }
    }
}