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

            // Hent brugeren for at få det personlige DailyGoal
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Bruger ikke fundet");

            // Hent alle logs for brugeren
            var allLogs = await _context.TaskLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .ToListAsync();

            // Find dagens tal direkte fra loggen
            var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);

            int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;

            // Brug brugerens personlige mål, ellers standard 3
            int currentDailyGoal = user.DailyGoal > 0 ? user.DailyGoal : 3;

            // 1. STREAK BEREGNING
            int streak = 0;

            // Tjek i dag mod det aktuelle mål
            if (effectiveDoneToday >= currentDailyGoal)
            {
                streak++;
            }

            // Tjek historikken bagud (her bruger vi loggens historiske mål,
            // så en mål-ændring i dag ikke ødelægger gamle streaks)
            var historyForStreak = allLogs.Where(l => l.Date < todayUtc).ToList();
            foreach (var log in historyForStreak)
            {
                if (log.TasksCompleted >= log.DailyGoal) streak++;
                else break;
            }

            // 2. TOTAL SCORE (Samlet sum af alle optjente point i loggen)
            int totalPoints = allLogs.Sum(l => l.PointsEarned);

            return Ok(new
            {
                Streak = streak,
                TotalPoints = totalPoints,
                TodayCompleted = effectiveDoneToday,
                DailyGoal = currentDailyGoal,
                // Indeholder de 7 nyeste dage inklusiv i dag til grafen
                RecentLogs = allLogs.Take(7)
            });
        }
    }
}