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

        // Hent det aktive mål og den nuværende point-saldo
        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetSavingsStatus(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Bruger ikke fundet");

            var activeGoal = await _context.SavingsGoals
                .Where(g => g.UserId == userId && !g.IsReached)
                .OrderByDescending(g => g.CreatedAt)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                CurrentPoints = user.TotalPoints,
                ActiveGoal = activeGoal
            });
        }

        // Opret et nyt opsparingsmål
        [HttpPost("goal")]
        public async Task<IActionResult> CreateGoal([FromBody] SavingsGoal goal)
        {
            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();
            return Ok(goal);
        }

        // Markér et mål som nået/købt
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

        // Udbetaling: Når forælderen giver gaven/pengene, trækkes pointene fra saldoen
        [HttpPost("payout/{userId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> PayoutPoints(int userId, [FromBody] int pointsToDeduct)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.TotalPoints < pointsToDeduct)
                return BadRequest("Ikke nok point på kontoen");

            user.TotalPoints -= pointsToDeduct;

            // Vi opretter en log-post med negative point, så statistikken (TotalPoints) i dashboardet stemmer overens
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

        // Justering af point manuelt (f.eks. hvis forælderen vil give ekstra point eller rette en fejl)
        [HttpPost("adjust-points/{userId}")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> AdjustPoints(int userId, [FromBody] int adjustment)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Opdater saldoen
            user.TotalPoints += adjustment;

            // Sørg for at saldoen ikke bliver negativ (valgfrit)
            if (user.TotalPoints < 0) user.TotalPoints = 0;

            // Opret en log-post for overblikkets skyld
            var log = new TaskLog
            {
                UserId = userId,
                Date = DateTime.UtcNow.Date,
                PointsEarned = adjustment,
                TasksCompleted = 0,
                DailyGoal = user.DailyGoal,
                // Du kan eventuelt tilføje en Note-kolonne i din database senere:
                // Note = "Manuel justering af forældre"
            };

            _context.TaskLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { NewTotalPoints = user.TotalPoints });
        }
    }
}