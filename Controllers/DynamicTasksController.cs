using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using ToDoAPI.Models;

namespace ToDoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DynamicTask>>> GetDynamicTasks()
        {
            return await _context.DynamicTasks.ToListAsync();
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
                if (!_context.DynamicTasks.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

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
    }
}
