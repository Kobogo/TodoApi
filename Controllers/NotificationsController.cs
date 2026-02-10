using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TodoApi.Models;
using TodoApi.Data; // Ret til din Data-mappe
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // [Authorize] // Udkommenteret for test
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionJSON model)
        {
            Console.WriteLine("--- SUBSCRIBE METODE RAMT ---");

            if (model == null) {
                Console.WriteLine("❌ Modtaget model er NULL!");
                return BadRequest("Data kunne ikke læses");
            }

            Console.WriteLine($"✅ Modtog endpoint: {model.Endpoint}");

            // Manuel udpakning af UserId for test (hvis Authorize er slået fra)
            // Hvis du tester uden login, så indsæt et fast ID (f.eks. 1) for at se om det virker
            int userId = 1;

            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint);

            if (existing == null)
            {
                var entity = new PushSubscriptionEntity
                {
                    UserId = userId,
                    Endpoint = model.Endpoint,
                    P256dh = model.Keys?.P256dh ?? "", // Brug ?. for at undgå crash hvis Keys mangler
                    Auth = model.Keys?.Auth ?? "",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PushSubscriptions.Add(entity);
                Console.WriteLine("💾 Forsøger at gemme ny subscription...");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("🎉 Gemt i database!");
            return Ok(new { message = "Subscription gemt succesfuldt!" });
        }
    }
}