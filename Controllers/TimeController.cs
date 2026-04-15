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
            var now = DateTime.UtcNow;
            var today = now.Date;
            var lastUpdateDate = user.LastTimerUpdate.ToUniversalTime().Date;

            // Hvis det er en ny dag siden sidst
            if (lastUpdateDate < today)
            {
                // 1. HØST: Gem overskydende tid til lørdagspuljen
                if (user.MinutesLeftToday > 0 && !user.IsPaused)
                {
                    user.SaturdayBonusPot += user.MinutesLeftToday;
                }

                // 2. NULSTIL STATISTIK
                user.BonusMinutesEarnedToday = 0;

                // 3. FIND BASIS-TID (Hverdag: 240, Weekend: 300)
                int baseMinutes = (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday) ? 300 : 240;

                // 4. LØRDAGS-SPECIAL: Sæt minutterne til 0 hvis ferie-mode er aktiveret, ellers tøm puljen ind i dagens tid hvis det er lørdag
                if (user.IsPaused)
                {
                    user.MinutesLeftToday = 0;
                }
                else if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    user.MinutesLeftToday = baseMinutes + user.SaturdayBonusPot;
                    user.SaturdayBonusPot = 0;
                }
                else
                {
                    user.MinutesLeftToday = baseMinutes;
                }

                // Opdater timestamp så vi ikke nulstiller igen i dag
                user.LastTimerUpdate = now;

                // Vi kalder ikke SaveChangesAsync her, da det gøres i de kaldende metoder
                return true;
            }

            return false;
        }
    }
}