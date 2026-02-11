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
            var todayUtc = DateTime.UtcNow.Date;

            // Hent alle logs for at beregne historik og streak
            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            var historyLogs = allLogs.Where(l => l.Date < todayUtc).ToList();
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);

            // 1. Samlet historisk score (før i dag)
            int totalPointsFromHistory = historyLogs.Sum(l => l.PointsEarned);

            // 2. Dagens præstation
            // Vi bruger nu loggen som "source of truth".
            // Hvis barnet har udført noget og derefter fjernet fluebenet eller slettet opgaven,
            // så vil loggen stadig have tallene gemt.
            int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;
            int pointsToday = todayLog?.PointsEarned ?? 0;

            int dailyGoal = 3;

            // 3. Streak beregning
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

            // 4. Samlet total (Historik + Dagens "låste" point)
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