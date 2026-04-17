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
    public class SavingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SavingsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. HENT STATUS (Opdateret til at returnere en liste af aktive mål)
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetSavingsStatus(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Bruger ikke fundet");

            // Vi henter alle mål der ikke er nået endnu for denne bruger
            var activeGoals = await _context.SavingsGoals
                .Where(g => g.UserId == userId && !g.IsReached)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(); // Ændret fra FirstOrDefault til ToList

            return Ok(new
            {
                CurrentPoints = user.TotalPoints,
                ActiveGoals = activeGoals // Sørg for at JSON-nøglen matcher din frontend (ActiveGoals)
            });
        }

        // 2. OPRET MÅL (Uændret, men vigtig for logikken)
        [HttpPost("goal")]
        public async Task<IActionResult> CreateGoal([FromBody] SavingsGoal goal)
        {
            // Sørg for at CreatedAt sættes på serveren for en sikkerheds skyld
            goal.CreatedAt = DateTime.UtcNow;
            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();
            return Ok(goal);
        }

        // 3. SLET MÅL (Ny funktion - kun forældre)
        [HttpDelete("goal/{id}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var goal = await _context.SavingsGoals.FindAsync(id);
            if (goal == null) return NotFound("Målet blev ikke fundet");

            _context.SavingsGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 4. MARKÉR SOM NÅET (Patch)
        [HttpPatch("goal/{id}/reached")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> MarkAsReached(int id)
        {
            var goal = await _context.SavingsGoals.FindAsync(id);
            if (goal == null) return NotFound();

            goal.IsReached = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 5. UDBETALING (Post)
        [HttpPost("payout/{userId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> PayoutPoints(int userId, [FromBody] int pointsToDeduct)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.TotalPoints < pointsToDeduct)
                return BadRequest("Ikke nok point på kontoen");

            user.TotalPoints -= pointsToDeduct;

            var today = DateTime.UtcNow.Date;
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);

            if (log != null) {
                log.PointsEarned -= pointsToDeduct;
            } else {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    PointsEarned = -pointsToDeduct,
                    TasksCompleted = 0,
                    DailyGoal = user.DailyGoal
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { NewTotalPoints = user.TotalPoints });
        }

        // 6. JUSTERING AF POINT
        [HttpPost("adjust-points/{userId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> AdjustPoints(int userId, [FromBody] int adjustment)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.TotalPoints += adjustment;
            if (user.TotalPoints < 0) user.TotalPoints = 0;

            _context.TaskLogs.Add(new TaskLog
            {
                UserId = userId,
                Date = DateTime.UtcNow.Date,
                PointsEarned = adjustment,
                TasksCompleted = 0,
                DailyGoal = user.DailyGoal
            });

            await _context.SaveChangesAsync();
            return Ok(new { NewTotalPoints = user.TotalPoints });
        }
    }
}