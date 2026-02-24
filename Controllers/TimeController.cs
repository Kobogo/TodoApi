using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetTimerStatus(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Vi kører lige et tjek her også, så dashboardet altid viser korrekt data
            await CheckAndResetDay(user);

            return Ok(new {
                user.MinutesLeftToday,
                user.IsTimerRunning,
                user.SaturdayBonusPot,
                user.BonusMinutesEarnedToday
            });
        }

        [HttpPatch("{userId}/sync")]
        public async Task<IActionResult> SyncTimer(int userId, [FromBody] int minutesUsed)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.MinutesLeftToday -= minutesUsed;
            if (user.MinutesLeftToday < 0) user.MinutesLeftToday = 0;

            user.LastTimerUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { user.MinutesLeftToday });
        }

        [HttpPatch("{userId}/toggle")]
        public async Task<IActionResult> ToggleTimer(int userId, [FromBody] bool running)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsTimerRunning = running;
            await _context.SaveChangesAsync();

            return Ok(new { user.IsTimerRunning });
        }

        [HttpPost("reset-daily-time/{userId}")]
        public async Task<IActionResult> ResetDailyTime(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            bool wasReset = await CheckAndResetDay(user);

            return Ok(new {
                user.MinutesLeftToday,
                user.SaturdayBonusPot,
                wasReset,
                Message = wasReset ? "Systemet er synkroniseret til den nye dag." : "Allerede opdateret for i dag."
            });
        }

        // HJÆLPEFUNKTION: Denne sørger for den komplekse logik omkring dage og lørdagspulje
        private async Task<bool> CheckAndResetDay(User user)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var lastUpdate = user.LastTimerUpdate.ToUniversalTime().Date;

            if (lastUpdate < today)
            {
                // 1. HØST: Hvis han ikke har brugt sin tid i går (eller de sidste mange dage)
                // Overfør kun hvis det ikke er lørdag (for lørdag tømmer vi puljen)
                if (user.MinutesLeftToday > 0)
                {
                    user.SaturdayBonusPot += user.MinutesLeftToday;
                }

                // 2. NULSTIL STATISTIK
                user.BonusMinutesEarnedToday = 0;

                // 3. FIND BASIS-TID FOR DEN NYE DAG
                int baseMinutes = (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday) ? 300 : 240;

                // 4. LØRDAGS-SPECIAL: Tøm puljen ind i dagens tid
                if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    user.MinutesLeftToday = baseMinutes + user.SaturdayBonusPot;
                    user.SaturdayBonusPot = 0; // Tøm puljen
                }
                else
                {
                    user.MinutesLeftToday = baseMinutes;
                }

                user.LastTimerUpdate = now;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        [HttpPatch("{userId}/add")]
        public async Task<IActionResult> AddExtraTime(int userId, [FromBody] int bonusMinutes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.MinutesLeftToday += bonusMinutes;
            user.BonusMinutesEarnedToday += bonusMinutes;

            await _context.SaveChangesAsync();

            return Ok(new {
                user.MinutesLeftToday,
                user.BonusMinutesEarnedToday
            });
        }
    }
}