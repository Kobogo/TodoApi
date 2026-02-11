using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TodoApi.Models;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers
{
    [Authorize] // Re-aktiveret: Nu kræves gyldigt token for alle kald i denne controller
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionJSON model)
        {
            // 1. Udlæs UserId fra JWT token (ClaimTypes.NameIdentifier er standard for bruger-ID)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Console.WriteLine("❌ Kunne ikke finde UserId i Token");
                return Unauthorized(new { message = "Ugyldigt bruger-id i token" });
            }

            if (model == null || string.IsNullOrEmpty(model.Endpoint)) {
                return BadRequest("Ugyldig subscription data");
            }

            // 2. Tjek om denne specifikke browser-endpoint allerede findes
            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint);

            if (existing == null)
            {
                // 3. Opret ny hvis den ikke findes
                var entity = new PushSubscriptionEntity
                {
                    UserId = userId,
                    Endpoint = model.Endpoint,
                    P256dh = model.Keys?.P256dh ?? "",
                    Auth = model.Keys?.Auth ?? "",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PushSubscriptions.Add(entity);
                Console.WriteLine($"💾 Gemmer ny Web Push for bruger {userId}");
            }
            else
            {
                // 4. Opdater eksisterende (hvis f.eks. en ny bruger logger ind på samme browser)
                existing.UserId = userId;
                existing.CreatedAt = DateTime.UtcNow;
                Console.WriteLine($"🔄 Opdaterer eksisterende Web Push for bruger {userId}");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subscription gemt succesfuldt!" });
        }

        [HttpPatch("complete/{taskId}")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            // Find opgaven i enten statiske eller dynamiske tasks
            var sTask = await _context.StaticTasks.FindAsync(taskId);
            if (sTask != null) sTask.IsCompleted = true;

            var dTask = await _context.DynamicTasks.FindAsync(taskId);
            if (dTask != null) dTask.IsCompleted = true;

            if (sTask == null && dTask == null) return NotFound();

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("snooze/{taskId}")]
        [Authorize]
        public async Task<IActionResult> SnoozeTask(int taskId)
        {
            // Hent nuværende tid i DK
            var dkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var nu = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dkTimeZone);

            // Tilføj 10 minutter
            var nyTid = nu.AddMinutes(10);
            var roundedSnooze = new TimeSpan(nyTid.Hour, nyTid.Minute, 0);

            // Find opgaverne
            var sTask = await _context.StaticTasks.FindAsync(taskId);
            var dTask = await _context.DynamicTasks.FindAsync(taskId);

            if (sTask == null && dTask == null)
            {
                return NotFound(new { message = "Opgave ikke fundet" });
            }

            if (sTask != null) sTask.TimeOfDay = roundedSnooze;
            if (dTask != null) dTask.TimeOfDay = roundedSnooze;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Snoozet til " + roundedSnooze.ToString(@"hh\:mm") });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Kunne ikke gemme snooze", error = ex.Message });
            }
        }
    }
}