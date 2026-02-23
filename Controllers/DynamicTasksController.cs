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
                    // Her sikrer vi at vi bruger BonusMinutes (husk at tjekke din model hedder dette)
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
                Console.WriteLine($"Fejl ved opdatering af completion: {ex.Message}");
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

        private bool DynamicTaskExists(int id) => _context.DynamicTasks.Any(e => e.Id == id);
    }
}