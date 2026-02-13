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

            return Ok(new {
                user.MinutesLeftToday,
                user.IsTimerRunning,
                user.SaturdayBonusPot,
                user.BonusMinutesEarnedToday // Tilføjet så dashboardet kan vise dagens optjening
            });
        }

        [HttpPatch("{userId}/sync")]
        public async Task<IActionResult> SyncTimer(int userId, [FromBody] int minutesUsed)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.MinutesLeftToday -= minutesUsed;
            user.LastTimerUpdate = DateTime.Now;

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

            var today = DateTime.Now;

            // Tjek om vi er gået ind i et nyt døgn
            if (user.LastTimerUpdate.Date < today.Date)
            {
                // 1. HØST OVERSKYDENDE TID til SaturdayBonusPot
                if (user.MinutesLeftToday > 0)
                {
                    user.SaturdayBonusPot += user.MinutesLeftToday;
                }

                // 2. NULSTIL DAGENS STATISTIK
                user.BonusMinutesEarnedToday = 0;

                // 3. FIND DEN NYE BASIS-TID
                int baseMinutes = 240; // Hverdage

                if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    // Lørdag: 5 timer + opsparing
                    baseMinutes = 300 + user.SaturdayBonusPot;
                    user.SaturdayBonusPot = 0;
                }
                else if (today.DayOfWeek == DayOfWeek.Sunday)
                {
                    baseMinutes = 300; // Søndag: 5 timer
                }

                user.MinutesLeftToday = baseMinutes;
                user.LastTimerUpdate = today;

                await _context.SaveChangesAsync();
                return Ok(new {
                    user.MinutesLeftToday,
                    user.SaturdayBonusPot,
                    user.BonusMinutesEarnedToday,
                    Message = "Dagen er nulstillet. Overskydende tid er flyttet til lørdagspuljen."
                });
            }

            return Ok(new { user.MinutesLeftToday, Message = "Tiden er allerede sat for i dag." });
        }

        // Bruges når han løser opgaver i Todo-appen
        [HttpPatch("{userId}/add")]
        public async Task<IActionResult> AddExtraTime(int userId, [FromBody] int bonusMinutes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Læg minutterne til den nuværende pulje
            user.MinutesLeftToday += bonusMinutes;

            // Tæl også med i statistikken for hvad han har tjent I DAG
            user.BonusMinutesEarnedToday += bonusMinutes;

            await _context.SaveChangesAsync();

            return Ok(new {
                user.MinutesLeftToday,
                user.BonusMinutesEarnedToday
            });
        }
    }
}