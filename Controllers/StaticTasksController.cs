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

        // GET: api/StaticTasks?userId=5
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            // Vi henter listen i to trin for at sikre, at PostgreSQL håndterer NULL korrekt.
            // Først definerer vi basen: alle globale opgaver.
            var query = _context.StaticTasks.AsQueryable();

            List<StaticTask> tasks;

            if (userId.HasValue)
            {
                // Hent opgaver der enten er fælles (null) ELLER tilhører brugeren
                tasks = await query
                    .Where(t => t.UserId == null || t.UserId == userId.Value)
                    .ToListAsync();
            }
            else
            {
                // Hvis intet userId sendes, henter vi kun de globale
                tasks = await query
                    .Where(t => t.UserId == null)
                    .ToListAsync();
            }

            return Ok(tasks);
        }

        // POST: api/StaticTasks
        [HttpPost]
        public async Task<ActionResult<StaticTask>> PostStaticTask([FromBody] StaticTask task)
        {
            _context.StaticTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { userId = task.UserId }, task);
        }

        // PUT: api/StaticTasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaticTask(int id, [FromBody] StaticTask updatedTask)
        {
            if (id != updatedTask.Id)
            {
                return BadRequest("ID mismatch between URL and body.");
            }

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

        // PATCH: api/StaticTasks/{id}/completion
        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> UpdateCompletion(int id, [FromBody] bool isCompleted)
        {
            var task = await _context.StaticTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsCompleted = isCompleted;

            // Vi bruger DateTime.UtcNow for at undgå tidszone-problemer i PostgreSQL
            if (isCompleted) task.LastCompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/StaticTasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaticTask(int id)
        {
            var task = await _context.StaticTasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.StaticTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StaticTaskExists(int id)
        {
            return _context.StaticTasks.Any(e => e.Id == id);
        }
    }
}