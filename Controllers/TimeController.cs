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

            // Vi kører et tjek ved hver hentning for at sikre friske data
            await CheckAndResetDay(user);

            return Ok(new {
                user.MinutesLeftToday,
                user.IsTimerRunning,
                user.SaturdayBonusPot,
                user.BonusMinutesEarnedToday,
                user.IsPaused
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

            await CheckAndResetDay(user);

            user.MinutesLeftToday = newMinutes;
            user.LastTimerUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new {
                user.MinutesLeftToday,
                Message = $"Tiden er manuelt opdateret til {newMinutes} minutter."
            });
        }

        // Forældre-override: Juster Lørdags-puljen manuelt
        [HttpPatch("{userId}/saturday-bonus")]
        public async Task<IActionResult> AdjustSaturdayBonus(int userId, [FromBody] int newBonus)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.SaturdayBonusPot = newBonus;
            await _context.SaveChangesAsync();

            return Ok(new {
                user.SaturdayBonusPot,
                Message = $"Lørdags-puljen er opdateret til {newBonus} minutter."
            });
        }

        // NY: Manuel nulstilling for HELE familien (bruges af Sync-knap)
        [HttpPost("reset-family-time/{familyId}")]
        public async Task<IActionResult> ResetFamilyTime(int familyId)
        {
            var members = await _context.Users
                .Where(u => u.FamilyId == familyId)
                .ToListAsync();

            if (members == null || !members.Any())
                return NotFound("Ingen familiemedlemmer fundet.");

            int resetCount = 0;
            foreach (var member in members)
            {
                bool wasReset = await CheckAndResetDay(member);
                if (wasReset) resetCount++;
            }

            await _context.SaveChangesAsync();

            return Ok(new {
                Message = $"Synkronisering fuldført. {resetCount} medlemmer opdateret til ny dag.",
                ResetCount = resetCount
            });
        }

        // Bruges når der løses opgaver for at give bonus
        [HttpPatch("{userId}/add")]
        public async Task<IActionResult> AddExtraTime(int userId, [FromBody] int bonusMinutes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.SaturdayBonusPot += bonusMinutes;
            user.BonusMinutesEarnedToday += bonusMinutes;

            await _context.SaveChangesAsync();

            return Ok(new {
                user.MinutesLeftToday,
                user.BonusMinutesEarnedToday,
                user.SaturdayBonusPot
            });
        }

        public class PauseDTO
        {
            public bool IsPaused { get; set; }
        }

        // Forældre-override: Sæt optjening på pause (Ferie mode)
        [HttpPatch("{userId}/pause")]
        public async Task<IActionResult> TogglePause(int userId, [FromBody] PauseDTO data)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            try
            {
                // Nu kan vi bare læse værdien direkte fra 'data'
                user.IsPaused = data.IsPaused;

                await _context.SaveChangesAsync();

                return Ok(new {
                    user.IsPaused,
                    Message = user.IsPaused ? "Ferie-mode aktiveret." : "Ferie-mode deaktiveret."
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Fejl ved opdatering: {ex.Message}");
            }
        }

        // HJÆLPEFUNKTION: Robust logik for dagsskifte og lørdagspulje
        private async Task<bool> CheckAndResetDay(User user)
        {
            // Vi bruger dansk tid (Central European Standard Time) for at sikre
            // at dagsskiftet sker ved midnat og ikke kl. 01:00/02:00 (UTC)
            var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var nowDanish = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, danishTimeZone);
            var today = nowDanish.Date;

            // Vi konverterer gemte LastTimerUpdate til dansk tid for sammenligning
            var lastUpdateDanish = TimeZoneInfo.ConvertTimeFromUtc(user.LastTimerUpdate, danishTimeZone).Date;

            if (lastUpdateDanish < today)
            {
                // 1. HØST KUN FRA HVERDAGE (Mandag - Torsdag + Fredag)
                // Vi gemmer kun tid hvis det var en hverdag i går, og ferie-mode er slukket
                bool wasWeekday = lastUpdateDanish.DayOfWeek != DayOfWeek.Saturday &&
                                lastUpdateDanish.DayOfWeek != DayOfWeek.Sunday;

                if (wasWeekday && user.MinutesLeftToday > 0 && !user.IsPaused)
                {
                    user.SaturdayBonusPot += user.MinutesLeftToday;
                }

                // 2. NULSTIL STATISTIK
                user.BonusMinutesEarnedToday = 0;

                // 3. FIND BASIS-TID (Hverdag: 240, Weekend: 300)
                int baseMinutes = (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday) ? 300 : 240;

                // 4. DAGSSKIFTE LOGIK
                if (user.IsPaused)
                {
                    // Ferie-mode: Ingen fast tid, men vi beholder puljen til senere
                    user.MinutesLeftToday = 0;
                }
                else if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    // Det er lørdag: Giv basis + tøm hele opsparingen
                    user.MinutesLeftToday = baseMinutes + user.SaturdayBonusPot;
                    user.SaturdayBonusPot = 0;
                }
                else
                {
                    // Almindelig dag (Søndag-Fredag)
                    user.MinutesLeftToday = baseMinutes;
                }

                // 5. VIGTIGT: Opdater timestamp til NU (UTC), så tjekket ikke kører igen før i morgen
                user.LastTimerUpdate = DateTime.UtcNow;

                // Gem ændringer med det samme for at undgå race-conditions
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}