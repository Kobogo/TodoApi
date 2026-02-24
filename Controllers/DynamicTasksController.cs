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
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DynamicTask>>> GetDynamicTasks([FromQuery] int? userId)
        {
            try
            {
                var query = _context.DynamicTasks.AsQueryable();
                if (userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId.Value);
                }

                var tasks = await query.ToListAsync();
                var today = DateTime.UtcNow.Date;
                bool changed = false;

                // Automatisk nulstilling ved nyt døgn
                foreach (var task in tasks.Where(t => t.IsCompleted && t.LastCompletedDate.HasValue))
                {
                    // 1. Tjek om opgaven blev løst før i dag
                    bool isOldCompletion = task.LastCompletedDate.Value.ToUniversalTime().Date < today;

                    // 2. Tjek om opgaven faktisk skal gentages (repeatDays må ikke være tom eller null)
                    bool isRecurring = !string.IsNullOrWhiteSpace(task.RepeatDays?.ToString());

                    if (isOldCompletion && isRecurring)
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
                Console.WriteLine($"Fejl i GetDynamicTasks: {ex.Message}");
                return StatusCode(500, "Kunne ikke hente opgaver");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DynamicTask>> GetDynamicTask(int id)
        {
            var task = await _context.DynamicTasks.FindAsync(id);
            if (task == null) return NotFound();
            return task;
        }

        [HttpPost]
        public async Task<ActionResult<DynamicTask>> PostDynamicTask(DynamicTask task)
        {
            _context.DynamicTasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDynamicTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDynamicTask(int id, DynamicTask task)
        {
            if (id != task.Id) return BadRequest();
            _context.Entry(task).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) {
                if (!DynamicTaskExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromBody] bool isCompleted)
        {
            try
            {
                var task = await _context.DynamicTasks.FindAsync(id);
                if (task == null) return NotFound();

                if (isCompleted && !task.IsCompleted) {
                    task.IsCompleted = true;
                    task.LastCompletedDate = DateTime.UtcNow;
                    await EnsureTaskLoggedAndAddBonus(task.UserId, 1, task.Points, task.TimeBonusMinutes ?? 0);
                }
                else if (!isCompleted) {
                    task.IsCompleted = false;
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Fejl ved opdatering");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDynamicTask(int id)
        {
            var task = await _context.DynamicTasks.FindAsync(id);
            if (task == null) return NotFound();
            _context.DynamicTasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task EnsureTaskLoggedAndAddBonus(int userId, int count, int points, int bonusMinutes)
        {
            var today = DateTime.UtcNow.Date;
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);
            var user = await _context.Users.FindAsync(userId);

            if (user != null) {
                user.TotalPoints += points;
                if (user.Role == "Child") {
                    user.MinutesLeftToday += bonusMinutes;
                    user.BonusMinutesEarnedToday += bonusMinutes;
                }
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

        private bool DynamicTaskExists(int id) => _context.DynamicTasks.Any(e => e.Id == id);
    }
}