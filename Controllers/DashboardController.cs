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
            // Hent alle historiske logs (inklusiv dem fra i dag, hvis vi begynder at logge løbende)
            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            // Skil i dag fra historikken (bruges til at undgå dobbelt-tælling af point)
            var todayDate = DateTime.Today;
            var historyLogs = allLogs.Where(l => l.Date < todayDate).ToList();
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayDate);

            // 1. Beregn basale point fra historikken
            int totalPointsFromHistory = historyLogs.Sum(l => l.PointsEarned);

            // 2. Tjek dagens status fra den aktive liste
            int activeDoneToday = await _context.DynamicTasks.CountAsync(t => t.UserId == userId && t.IsCompleted)
                                + await _context.StaticTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);

            // Hvis vi har en log for i dag (f.eks. fra slettede opgaver), skal vi sikre os,
            // at vi bruger det højeste tal, så point ikke forsvinder.
            int effectiveDoneToday = todayLog != null ? Math.Max(todayLog.TasksCompleted, activeDoneToday) : activeDoneToday;

            int dailyGoal = 3;
            int pointsToday = effectiveDoneToday * 10;

            // 3. Beregn Streak
            int streak = 0;

            // Hvis målet er nået i dag, tæller vi den med i streaken med det samme!
            if (effectiveDoneToday >= dailyGoal)
            {
                streak++;
            }

            // Tæl baglæns gennem historikken
            foreach (var log in historyLogs)
            {
                if (log.TasksCompleted >= log.DailyGoal) streak++;
                else break;
            }

            // 4. Samlet point (Historik + i dag)
            int liveTotalPoints = totalPointsFromHistory + pointsToday;

            return Ok(new
            {
                Streak = streak,
                TotalPoints = liveTotalPoints,
                TodayCompleted = effectiveDoneToday, // Denne falder nu ikke hvis man sletter, HVIS man gemmer sletningen i loggen
                DailyGoal = dailyGoal,
                RecentLogs = historyLogs.Take(7)
            });
        }
    }
}