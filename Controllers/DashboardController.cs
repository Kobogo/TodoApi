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
            // Vi bruger UtcNow.Date for at sikre ensartethed med databasen
            var todayUtc = DateTime.UtcNow.Date;

            // Hent historiske logs
            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            var historyLogs = allLogs.Where(l => l.Date < todayUtc).ToList();
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);

            // 1. Beregn basale point fra historikken
            int totalPointsFromHistory = historyLogs.Sum(l => l.PointsEarned);

            // 2. Tjek dagens status (Kun opgaver fuldført I DAG tæller)
            int dynamicDone = await _context.DynamicTasks
                .CountAsync(t => t.UserId == userId &&
                                 t.IsCompleted &&
                                 t.LastCompletedDate >= todayUtc);

            int staticDone = await _context.StaticTasks
                .CountAsync(t => (t.UserId == userId || t.UserId == null) &&
                                 t.IsCompleted &&
                                 t.LastCompletedDate >= todayUtc);

            int activeDoneToday = dynamicDone + staticDone;

            // ANTI-CHEAT / ANTI-DROP:
            // Vi bruger det højeste tal mellem loggen (gemte point) og den aktive liste.
            // Hvis man sletter en opgave eller fjerner et flueben, vil loggen 'vinde',
            // så barnets TodayCompleted ikke falder.
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