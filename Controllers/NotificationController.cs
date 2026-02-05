using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TodoApi.Models;
using TodoApi.Data; // Ret til din Data-mappe
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Controllers
{
    [Authorize] // Kun logget ind brugere kan abonnere
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionJSON model)
        {
            // 1. Find den nuværende brugers ID (fra JWT token)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Tjek om denne subscription allerede findes for at undgå dubletter
            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint);

            if (existing == null)
            {
                // 3. Opret en ny række i databasen
                var entity = new PushSubscriptionEntity
                {
                    UserId = userId,
                    Endpoint = model.Endpoint,
                    P256dh = model.Keys.P256dh,
                    Auth = model.Keys.Auth
                };

                _context.PushSubscriptions.Add(entity);
            }
            else
            {
                // Opdater UserId hvis det er en eksisterende endpoint der har skiftet bruger
                existing.UserId = userId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subscription gemt succesfuldt!" });
        }
    }
}