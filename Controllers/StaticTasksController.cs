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

        public StaticTasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            var query = _context.StaticTasks.AsQueryable();
            List<StaticTask> tasks;

            if (userId.HasValue)
            {
                tasks = await query.Where(t => t.UserId == null || t.UserId == userId.Value).ToListAsync();
            }
            else
            {
                tasks = await query.Where(t => t.UserId == null).ToListAsync();
            }

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<StaticTask>> PostStaticTask([FromBody] StaticTask task)
        {
            _context.StaticTasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { userId = task.UserId }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaticTask(int id, [FromBody] StaticTask updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest("ID mismatch");

            // Tjekker om opgaven findes uden at tracke den, så vi kan overskrive den
            var exists = await _context.StaticTasks.AnyAsync(t => t.Id == id);
            if (!exists) return NotFound();

            _context.Entry(updatedTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaticTaskExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromBody] bool isCompleted)
        {
            // RETTET: Brugte før DynamicTasks ved en fejl
            var task = await _context.StaticTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (isCompleted && !task.IsCompleted)
            {
                task.IsCompleted = true;
                task.LastCompletedDate = DateTime.UtcNow;

                if (task.UserId.HasValue)
                {
                    await EnsureTaskLogged(task.UserId.Value, 1, task.Points);
                }
            }
            else if (!isCompleted)
            {
                task.IsCompleted = false;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaticTask(int id)
        {
            var task = await _context.StaticTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.IsCompleted && task.UserId.HasValue)
            {
                await EnsureTaskLogged(task.UserId.Value, 1, task.Points);
            }

            _context.StaticTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hjælpemetode til logging
        private async Task EnsureTaskLogged(int userId, int count, int points)
        {
            var today = DateTime.UtcNow.Date;
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);

            if (log == null)
            {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = count,
                    DailyGoal = 3,
                    PointsEarned = points
                });
            }
            else
            {
                log.TasksCompleted += count;
                log.PointsEarned += points;
            }
            await _context.SaveChangesAsync();
        }

        private bool StaticTaskExists(int id)
        {
            return _context.StaticTasks.Any(e => e.Id == id);
        }
    }
}