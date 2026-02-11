using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DynamicTask>>> GetDynamicTasks([FromQuery] int? userId)
        {
            var query = _context.DynamicTasks.AsQueryable();
            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }
            return await query.ToListAsync();
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
            catch (DbUpdateConcurrencyException)
            {
                if (!DynamicTaskExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromBody] bool isCompleted)
        {
            var task = await _context.DynamicTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsCompleted = isCompleted;
            if (isCompleted) task.LastCompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDynamicTask(int id)
        {
            var task = await _context.DynamicTasks.FindAsync(id);
            if (task == null) return NotFound();

            // NYT: Hvis opgaven slettes, mens den er færdig, skal vi gemme indsatsen i loggen
            if (task.IsCompleted)
            {
                await EnsureTaskLogged(task.UserId, 1);
            }

            _context.DynamicTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hjælpefunktion til at sikre at point og tælling gemmes ved sletning
        private async Task EnsureTaskLogged(int userId, int count)
        {
            var today = DateTime.Today;
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);

            if (log == null)
            {
                _context.TaskLogs.Add(new TaskLog
                {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = count,
                    DailyGoal = 3,
                    PointsEarned = count * 10
                });
            }
            else
            {
                log.TasksCompleted += count;
                log.PointsEarned += (count * 10);
            }
        }

        private bool DynamicTaskExists(int id)
        {
            return _context.DynamicTasks.Any(e => e.Id == id);
        }
    }
}