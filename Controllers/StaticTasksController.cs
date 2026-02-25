using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StaticTasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaticTasksController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            try
            {
                var query = _context.StaticTasks.AsQueryable();
                List<StaticTask> tasks;

                // Henter opgaver der enten er fælles (UserId == null) eller tilhører den valgte bruger
                if (userId.HasValue) {
                    tasks = await query.Where(t => t.UserId == null || t.UserId == userId.Value).ToListAsync();
                } else {
                    tasks = await query.Where(t => t.UserId == null).ToListAsync();
                }

                var today = DateTime.UtcNow.Date;
                bool changed = false;

                foreach (var task in tasks.Where(t => t.IsCompleted && t.LastCompletedDate.HasValue))
                {
                    if (task.LastCompletedDate.Value.ToUniversalTime().Date < today)
                    {
                        task.IsCompleted = false;
                        _context.Entry(task).Property(x => x.IsCompleted).IsModified = true;
                        changed = true;
                    }
                }

                if (changed) await _context.SaveChangesAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl i GetAll: {ex.Message}");
                return StatusCode(500, "Intern serverfejl");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StaticTask>> PostStaticTask([FromBody] StaticTask task)
        {
            _context.StaticTasks.Add(task);
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaticTask(int id, [FromBody] StaticTask updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest("ID mismatch");
            _context.Entry(updatedTask).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) {
                if (!StaticTaskExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // OPDATERET: Bruger nu performingUserId for at sikre, at point tildeles den person, der trykker
        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromQuery] int performingUserId, [FromBody] bool isCompleted)
        {
            try
            {
                var task = await _context.StaticTasks.FindAsync(id);
                if (task == null) return NotFound();

                if (isCompleted && !task.IsCompleted)
                {
                    task.IsCompleted = true;
                    task.LastCompletedDate = DateTime.UtcNow;

                    // Giv point og bonus til den udførende bruger (f.eks. Far eller Barn)
                    await EnsureTaskLoggedAndAddBonus(performingUserId, 1, task.Points, task.TimeBonusMinutes ?? 0);
                }
                else if (!isCompleted)
                {
                    task.IsCompleted = false;
                    // Her kan man evt. tilføje logik til at trække point fra igen, hvis det ønskes
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Vigtigt: Log fejlen så du kan se den i Render console
                Console.WriteLine($"Fejl ved opdatering af completion for opgave {id}: {ex.Message}");
                return StatusCode(500, $"Fejl ved opdatering: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaticTask(int id)
        {
            var task = await _context.StaticTasks.FindAsync(id);
            if (task == null) return NotFound();
            _context.StaticTasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task EnsureTaskLoggedAndAddBonus(int userId, int count, int points, int bonusMinutes)
        {
            var today = DateTime.UtcNow.Date;

            // Hent brugeren først for at sikre, at vedkommende eksisterer
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception($"Bruger med ID {userId} blev ikke fundet.");

            // Opdater brugerens totale point og skærmtid (hvis barn)
            user.TotalPoints += points;
            if (user.Role == "Child")
            {
                user.MinutesLeftToday += bonusMinutes;
                user.BonusMinutesEarnedToday += bonusMinutes;
            }

            // Hent eller opret dags-log
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);

            if (log == null)
            {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = count,
                    DailyGoal = user.DailyGoal,
                    PointsEarned = points
                });
            }
            else
            {
                log.TasksCompleted += count;
                log.PointsEarned += points;
            }
        }

        private bool StaticTaskExists(int id) => _context.StaticTasks.Any(e => e.Id == id);
    }
}