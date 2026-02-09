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
            // 1. Find den nuværende brugers ID (fra JWT token) som string
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // 2. Tjek om denne subscription allerede findes
            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint);

            if (existing == null)
            {
                // 3. Opret en ny række - UserId er nu en int
                var entity = new PushSubscriptionEntity
                {
                    UserId = userId, // Nu en int
                    Endpoint = model.Endpoint,
                    P256dh = model.Keys.P256dh,
                    Auth = model.Keys.Auth,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PushSubscriptions.Add(entity);
            }
            else
            {
                // Opdater UserId hvis eksisterende endpoint har skiftet bruger
                existing.UserId = userId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subscription gemt succesfuldt!" });
        }
    }
}