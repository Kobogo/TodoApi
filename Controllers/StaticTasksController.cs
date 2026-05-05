using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services; // Tilføjet
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StaticTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAchievementService _achievementService; // Tilføjet

        public StaticTasksController(AppDbContext context, IAchievementService achievementService)
        {
            _context = context;
            _achievementService = achievementService; // Tilføjet
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            try
            {
                var query = _context.StaticTasks.AsQueryable();
                List<StaticTask> tasks;

                if (userId.HasValue) {
                    tasks = await query.Where(t => t.UserId == null || t.UserId == userId.Value).ToListAsync();
                } else {
                    tasks = await query.Where(t => t.UserId == null).ToListAsync();
                }

                var today = DateTime.UtcNow.Date;
                bool changed = false;

                foreach (var task in tasks.Where(t => t.IsCompleted && t.LastCompletedDate.HasValue))
                {
                    if (task.LastCompletedDate.Value.ToUniversalTime().Date < today)
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
                return StatusCode(500, "Intern serverfejl");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StaticTask>> PostStaticTask([FromBody] StaticTask task)
        {
            _context.StaticTasks.Add(task);
            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> UpdateCompletion(int id, [FromQuery] int performingUserId, [FromBody] bool isCompleted, [FromQuery] int count = 1)
        {
            try
            {
                var task = await _context.StaticTasks.FindAsync(id);
                if (task == null) return NotFound();

                if (isCompleted)
                {
                    task.IsCompleted = true;
                    task.LastCompletedDate = DateTime.UtcNow;
                    await HandleUserStatsUpdate(performingUserId, true, task.Points * count, (task.TimeBonusMinutes ?? 0) * count);

                    // Tjek for achievements
                    await _achievementService.CheckAndAwardAchievementsAsync(performingUserId, "Tasks");
                }
                else
                {
                    task.IsCompleted = false;
                    await HandleUserStatsUpdate(performingUserId, false, task.Points * count, (task.TimeBonusMinutes ?? 0) * count);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fejl: {ex.Message}");
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

        private async Task HandleUserStatsUpdate(int userId, bool adding, int points, int bonusMinutes)
        {
            var today = DateTime.UtcNow.Date;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var log = await _context.TaskLogs.FirstOrDefaultAsync(l => l.UserId == userId && l.Date == today);
            int multiplier = adding ? 1 : -1;

            user.TotalPoints += (points * multiplier);
            if (user.Role == "Child")
            {
                user.MinutesLeftToday += (bonusMinutes * multiplier);
                user.BonusMinutesEarnedToday += (bonusMinutes * multiplier);
            }

            if (log == null && adding)
            {
                _context.TaskLogs.Add(new TaskLog {
                    UserId = userId,
                    Date = today,
                    TasksCompleted = 1,
                    DailyGoal = user.DailyGoal > 0 ? user.DailyGoal : 3,
                    PointsEarned = points
                });
            }
            else if (log != null)
            {
                log.TasksCompleted += multiplier;
                log.PointsEarned += (points * multiplier);

                if (log.TasksCompleted < 0) log.TasksCompleted = 0;
                if (log.PointsEarned < 0) log.PointsEarned = 0;
                if (user.TotalPoints < 0) user.TotalPoints = 0;
            }
        }

        private bool StaticTaskExists(int id) => _context.StaticTasks.Any(e => e.Id == id);
    }
}