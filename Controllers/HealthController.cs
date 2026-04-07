using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                // Vi laver et minimalt kald til Neon databasen for at holde den vågen.
                // CanConnectAsync tjekker om databasen svarer uden at hente data.
                bool canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new {
                        status = "Healthy",
                        database = "Connected",
                        timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(500, "Database connection failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }
    }
}