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
            try
            {
                var todayUtc = DateTime.UtcNow.Date;

                // 1. Hent brugeren først
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound("Bruger ikke fundet");

                // 2. Hent det aktive opsparingsmål (Hvor IsReached er false)
                var activeGoal = await _context.SavingsGoals
                    .AsNoTracking()
                    .Where(g => g.UserId == userId && !g.IsReached)
                    .OrderByDescending(g => g.CreatedAt)
                    .FirstOrDefaultAsync();

                // Brug målet fra bruger-tabellen (default til 3 hvis 0 eller mindre)
                int currentDailyGoal = user.DailyGoal > 0 ? user.DailyGoal : 3;

                // 3. Hent alle logs for brugeren
                var allLogs = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.Date)
                    .ToListAsync();

                // Find dagens tal direkte fra loggen
                var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);
                int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;

                // 4. STREAK BEREGNING
                int streak = 0;

                // Tjek i dag mod det aktuelle mål
                if (effectiveDoneToday >= currentDailyGoal)
                {
                    streak++;
                }

                // Tjek historikken bagud
                var historyForStreak = allLogs.Where(l => l.Date < todayUtc).ToList();
                foreach (var log in historyForStreak)
                {
                    int historicalGoal = log.DailyGoal > 0 ? log.DailyGoal : 3;

                    if (log.TasksCompleted >= historicalGoal)
                        streak++;
                    else
                        break;
                }

                // 5. TOTAL SCORE (Samlet sum af alle optjente point i loggen)
                int totalPoints = allLogs.Sum(l => l.PointsEarned);

                return Ok(new
                {
                    Streak = streak,
                    TotalPoints = totalPoints,
                    ActiveGoal = activeGoal, // Sendes nu med til frontenden
                    TodayCompleted = effectiveDoneToday,
                    DailyGoal = currentDailyGoal,
                    MinutesLeftToday = user.MinutesLeftToday,
                    BonusMinutesEarnedToday = user.BonusMinutesEarnedToday,
                    SaturdayBonusPot = user.SaturdayBonusPot,
                    IsTimerRunning = user.IsTimerRunning,
                    IsPaused = user.IsPaused,
                    RecentLogs = allLogs.Take(7).Select(l => new {
                        l.Date,
                        l.TasksCompleted,
                        DailyGoal = l.Date == todayUtc ? currentDailyGoal : (l.DailyGoal > 0 ? l.DailyGoal : 3)
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Fejl ved hentning af stats", error = ex.Message });
            }
        }
    }
}