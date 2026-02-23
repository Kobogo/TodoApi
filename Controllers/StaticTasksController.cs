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
                if (userId.HasValue)
                {
                    // Vi bruger .Where med eksplicit håndtering af null for at undgå LINQ-to-SQL fejl
                    tasks = await query.Where(t => t.UserId == null || t.UserId == userId.Value).ToListAsync();
                }
                else
                {
                    tasks = await query.Where(t => t.UserId == null).ToListAsync();
                }

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl i StaticTasks GetAll: {ex.Message}");
                return StatusCode(500, "Intern serverfejl");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StaticTask>> PostStaticTask([FromBody] StaticTask task)
        {
            _context.StaticTasks.Add(task);
            await _context.SaveChangesAsync();
            // Rettet: CreatedAtAction peger nu på GetAll (da vi ikke har en GetById endnu)
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

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromBody] bool isCompleted)
        {
            try
            {
                var task = await _context.StaticTasks.FindAsync(id);
                if (task == null) return NotFound();

                if (isCompleted && !task.IsCompleted) {
                    task.IsCompleted = true;
                    task.LastCompletedDate = DateTime.UtcNow;
                    if (task.UserId.HasValue) {
                        await EnsureTaskLoggedAndAddBonus(task.UserId.Value, 1, task.Points, task.BonusMinutes ?? 0);
                    }
                }
                else if (!isCompleted) {
                    task.IsCompleted = false;
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved opdatering af static completion: {ex.Message}");
                return StatusCode(500, "Fejl ved opdatering");
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
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);
            var user = await _context.Users.FindAsync(userId);

            if (user != null && user.Role == "Child") {
                user.MinutesLeftToday += bonusMinutes;
            }

            if (log == null) {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = count,
                    DailyGoal = user?.DailyGoal ?? 3,
                    PointsEarned = points
                });
            } else {
                log.TasksCompleted += count;
                log.PointsEarned += points;
            }
        }

        private bool StaticTaskExists(int id) => _context.StaticTasks.Any(e => e.Id == id);
    }
}