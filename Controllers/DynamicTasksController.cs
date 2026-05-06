using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services; // Tilføjet
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAchievementService _achievementService; // Tilføjet

        public TasksController(AppDbContext context, IAchievementService achievementService)
        {
            _context = context;
            _achievementService = achievementService; // Tilføjet
        }

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

                foreach (var task in tasks.Where(t => t.IsCompleted && t.LastCompletedDate.HasValue))
                {
                    bool isOldCompletion = task.LastCompletedDate.Value.ToUniversalTime().Date < today;
                    bool isRecurring = task.RepeatDays != null && task.RepeatDays.Any();

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

                if (isCompleted != task.IsCompleted)
                {
                    task.IsCompleted = isCompleted;
                    if (isCompleted)
                    {
                        task.LastCompletedDate = DateTime.UtcNow;
                    }

                    await HandleUserStatsUpdate(task.UserId, isCompleted, task.Points, task.TimeBonusMinutes ?? 0);

                    // Tjek for achievements når opgaven markeres som udført
                    if (isCompleted)
                    {
                        await _achievementService.CheckAndAwardAchievementsAsync(task.UserId);
                    }

                    await _context.SaveChangesAsync();
                }

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

        private async Task HandleUserStatsUpdate(int userId, bool adding, int points, int bonusMinutes)
        {
            var today = DateTime.UtcNow.Date;
            var user = await _context.Users.FindAsync(userId);
            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);

            int multiplier = adding ? 1 : -1;

            if (user != null)
            {
                user.TotalPoints += (points * multiplier);
                if (user.Role == "Child")
                {
                    user.MinutesLeftToday += (bonusMinutes * multiplier);
                    user.BonusMinutesEarnedToday += (bonusMinutes * multiplier);
                }
            }

            if (log == null && adding)
            {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = 1,
                    DailyGoal = user?.DailyGoal ?? 3,
                    PointsEarned = points
                });
            }
            else if (log != null)
            {
                log.TasksCompleted += multiplier;
                log.PointsEarned += (points * multiplier);

                if (log.TasksCompleted < 0) log.TasksCompleted = 0;
                if (log.PointsEarned < 0) log.PointsEarned = 0;
                if (user != null && user.TotalPoints < 0) user.TotalPoints = 0;
            }
        }

        private bool DynamicTaskExists(int id) => _context.DynamicTasks.Any(e => e.Id == id);
    }
}