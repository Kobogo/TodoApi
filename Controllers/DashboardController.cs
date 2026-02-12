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

            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            // Find dagens tal direkte fra loggen
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);

            int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;
            int pointsToday = todayLog?.PointsEarned ?? 0;
            int dailyGoal = 3;

            // STREAK (samme logik som før)
            int streak = 0;
            if (effectiveDoneToday >= dailyGoal) streak++;

            // Beregn streak bagud fra i går
            var historyForStreak = allLogs.Where(l => l.Date < todayUtc).ToList();
            foreach (var log in historyForStreak)
            {
                if (log.TasksCompleted >= log.DailyGoal) streak++;
                else break;
            }

            // TOTAL SCORE
            int totalPoints = allLogs.Sum(l => l.PointsEarned);

            return Ok(new
            {
                Streak = streak,
                TotalPoints = totalPoints,
                TodayCompleted = effectiveDoneToday,
                DailyGoal = dailyGoal,
                // RETTELSE: Tag de 7 nyeste logs INKLUSIV i dag
                RecentLogs = allLogs.Take(7)
            });
        }
    }
}