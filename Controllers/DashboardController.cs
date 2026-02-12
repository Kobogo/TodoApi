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

                // 1. Hent brugeren først for at få det absolut nyeste DailyGoal
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound("Bruger ikke fundet");

                // Brug målet fra bruger-tabellen (default til 3 hvis 0 eller mindre)
                int currentDailyGoal = user.DailyGoal > 0 ? user.DailyGoal : 3;

                // 2. Hent alle logs for brugeren
                var allLogs = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.Date)
                    .ToListAsync();

                // Find dagens tal direkte fra loggen
                var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);
                int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;

                // 3. STREAK BEREGNING
                int streak = 0;

                // Tjek i dag mod det aktuelle mål (det vi lige har hentet fra User)
                if (effectiveDoneToday >= currentDailyGoal)
                {
                    streak++;
                }

                // Tjek historikken bagud
                // Her bruger vi loggens historiske mål (log.DailyGoal),
                // så en ændring i dag ikke ødelægger gårsdagens streak retroaktivt.
                var historyForStreak = allLogs.Where(l => l.Date < todayUtc).ToList();
                foreach (var log in historyForStreak)
                {
                    // Hvis loggen ikke har et mål gemt (gamle data), brug 3 som fallback
                    int historicalGoal = log.DailyGoal > 0 ? log.DailyGoal : 3;

                    if (log.TasksCompleted >= historicalGoal)
                        streak++;
                    else
                        break;
                }

                // 4. TOTAL SCORE (Samlet sum af alle optjente point i loggen)
                int totalPoints = allLogs.Sum(l => l.PointsEarned);

                return Ok(new
                {
                    Streak = streak,
                    TotalPoints = totalPoints,
                    TodayCompleted = effectiveDoneToday,
                    DailyGoal = currentDailyGoal, // Returnerer det opdaterede mål
                    RecentLogs = allLogs.Take(7).Select(l => new {
                        l.Date,
                        l.TasksCompleted,
                        // Vi sender målet med for hver dag, så grafen kan vise det korrekt
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