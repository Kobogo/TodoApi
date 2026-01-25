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
            // Henter både globale opgaver (null) og brugerens egne opgaver
            var tasks = await _context.StaticTasks
                .Where(t => t.UserId == null || (userId.HasValue && t.UserId == userId.Value))
                .ToListAsync();

            return Ok(tasks);
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
            if (isCompleted) task.LastCompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StaticTaskExists(int id)
        {
            return _context.StaticTasks.Any(e => e.Id == id);
        }
    }
}