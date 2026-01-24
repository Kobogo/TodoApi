using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    [Authorize] // Kræver login
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tasks?userId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DynamicTask>>> GetDynamicTasks([FromQuery] int? userId)
        {
            // Hvis userId er sendt med (fra forældre-visning), filtrer på det.
            // Ellers henter vi alle (senere kan vi begrænse så børn kun ser egne).
            var query = _context.DynamicTasks.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DynamicTask>> GetDynamicTask(int id)
        {
            var task = await _context.DynamicTasks.FindAsync(id);

            if (task == null)
                return NotFound();

            return task;
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<DynamicTask>> PostDynamicTask(DynamicTask task)
        {
            _context.DynamicTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDynamicTask), new { id = task.Id }, task);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDynamicTask(int id, DynamicTask task)
        {
            if (id != task.Id)
                return BadRequest();

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DynamicTaskExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // PATCH: api/Tasks/5/completion
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

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDynamicTask(int id)
        {
            var task = await _context.DynamicTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.DynamicTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DynamicTaskExists(int id)
        {
            return _context.DynamicTasks.Any(e => e.Id == id);
        }
    }
}