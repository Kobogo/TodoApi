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

                // 1. Hent brugeren
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound("Bruger ikke fundet");

                // 2. Hent det aktive opsparingsmål
                var activeGoal = await _context.SavingsGoals
                    .AsNoTracking()
                    .Where(g => g.UserId == userId && !g.IsReached)
                    .OrderByDescending(g => g.CreatedAt)
                    .FirstOrDefaultAsync();

                int currentDailyGoal = user.DailyGoal > 0 ? user.DailyGoal : 3;

                // 3. Hent alle logs
                var allLogs = await _context.TaskLogs
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.Date)
                    .ToListAsync();

                var todayLog = allLogs.FirstOrDefault(l => l.Date == todayUtc);
                int effectiveDoneToday = todayLog?.TasksCompleted ?? 0;

                // 4. STREAK & BUFFER LOGIK
                int streak = 0;
                bool isBufferAvailable = false;
                int currentRunForBuffer = 0;
                bool bufferWasUsedInCalculations = false;

                // Vi tjekker historikken bagud (eksklusive i dag)
                var history = allLogs.Where(l => l.Date < todayUtc).ToList();

                // Beregn om brugeren har en buffer klar (har klaret de sidste 7 dage før i dag)
                int consecutiveBeforeToday = 0;
                foreach (var log in history)
                {
                    int goal = log.DailyGoal > 0 ? log.DailyGoal : 3;
                    if (log.TasksCompleted >= goal) consecutiveBeforeToday++;
                    else break;
                }

                isBufferAvailable = consecutiveBeforeToday >= 7;
                currentRunForBuffer = consecutiveBeforeToday % 7;

                // Beregn den faktiske streak (inkluder i dag hvis klaret)
                if (effectiveDoneToday >= currentDailyGoal)
                {
                    streak = 1 + consecutiveBeforeToday;
                }
                else
                {
                    // Hvis i dag IKKE er klaret, men vi har en buffer, lever streaken stadig
                    if (isBufferAvailable)
                    {
                        streak = 1 + consecutiveBeforeToday; // Vi tæller "i dag" med pga buffer
                        bufferWasUsedInCalculations = true;
                    }
                    else
                    {
                        // Ingen buffer og mål ikke nået i dag? Streaken er historikken.
                        streak = consecutiveBeforeToday;
                    }
                }

                // 5. TOTAL SCORE
                int totalPoints = user.TotalPoints;

                return Ok(new
                {
                    Streak = streak,
                    TotalPoints = totalPoints,
                    ActiveGoal = activeGoal,
                    TodayCompleted = effectiveDoneToday,
                    DailyGoal = currentDailyGoal,
                    IsBufferAvailable = isBufferAvailable,
                    DaysUntilBuffer = 7 - currentRunForBuffer,
                    BufferUsedToday = bufferWasUsedInCalculations,
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