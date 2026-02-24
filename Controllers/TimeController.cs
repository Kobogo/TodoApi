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

        // Henter status til både uret og dashboardet
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

        // Bruges til løbende synkronisering mens timeren tæller ned
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

        // Start/Stop af timeren
        [HttpPatch("{userId}/toggle")]
        public async Task<IActionResult> ToggleTimer(int userId, [FromBody] bool running)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsTimerRunning = running;
            await _context.SaveChangesAsync();

            return Ok(new { user.IsTimerRunning });
        }

        // Forældre-override: Juster tiden manuelt fra SettingsPage
        [HttpPatch("{userId}/adjust-time")]
        public async Task<IActionResult> AdjustTime(int userId, [FromBody] int newMinutes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Vi sikrer os at dagen er up-to-date før vi overskriver
            await CheckAndResetDay(user);

            user.MinutesLeftToday = newMinutes;
            user.LastTimerUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new {
                user.MinutesLeftToday,
                Message = $"Tiden er manuelt opdateret til {newMinutes} minutter."
            });
        }

        // Manuel nulstilling (bruges f.eks. af din Sync-knap)
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

        // Bruges når han løser opgaver i Todo-appen for at give bonus
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

        // HJÆLPEFUNKTION: Håndterer logikken for dagsskifte og lørdagspulje
        private async Task<bool> CheckAndResetDay(User user)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var lastUpdate = user.LastTimerUpdate.ToUniversalTime().Date;

            if (lastUpdate < today)
            {
                // 1. HØST: Hvis han har tid tilbage, gem det til lørdagspuljen
                if (user.MinutesLeftToday > 0)
                {
                    user.SaturdayBonusPot += user.MinutesLeftToday;
                }

                // 2. NULSTIL STATISTIK
                user.BonusMinutesEarnedToday = 0;

                // 3. FIND BASIS-TID (Hverdag: 240, Weekend: 300)
                int baseMinutes = (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday) ? 300 : 240;

                // 4. LØRDAGS-SPECIAL: Tøm puljen ind i dagens tid
                if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    user.MinutesLeftToday = baseMinutes + user.SaturdayBonusPot;
                    user.SaturdayBonusPot = 0;
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
    }
}