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

            return Ok(new {
                user.MinutesLeftToday,
                user.IsTimerRunning,
                user.SaturdayBonusPot
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

        [HttpPatch("{userId}/add")]
        public async Task<IActionResult> AddExtraTime(int userId, [FromBody] int bonusMinutes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.MinutesLeftToday += bonusMinutes;
            await _context.SaveChangesAsync();

            return Ok(new { user.MinutesLeftToday });
        }
    }
}