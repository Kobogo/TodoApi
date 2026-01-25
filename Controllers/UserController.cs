using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    // Vi definerer en simpel model til login-forespørgsler
    // Dette matcher præcis de JSON-navne, din React frontend sender
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginInfo)
        {
            try
            {
                // 1. Find brugeren i databasen
                // EF Core bruger nu din AppDbContext mapping til at lede efter 'username' kolonnen
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

                // 2. Tjek om brugeren findes og om kodeordet matcher
                if (user == null || user.PasswordHash != loginInfo.PasswordHash)
                {
                    return Unauthorized(new { message = "Forkert brugernavn eller adgangskode" });
                }

                // 3. Setup JWT Token generering
                var tokenHandler = new JwtSecurityTokenHandler();

                // Vi henter nøglen fra miljøvariabler (Render) eller bruger en fallback til lokal udvikling
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                             ?? "DIN_MEGET_LANGE_HEMMELIGE_NØGLE_HER_PÅ_MINDST_32_TEGN";
                var key = Encoding.ASCII.GetBytes(jwtKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("FamilyId", user.FamilyId?.ToString() ?? "0")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // 4. Returner token og brugeroplysninger i camelCase (pga. Program.cs indstillingen)
                return Ok(new
                {
                    Token = tokenString,
                    User = new
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Role = user.Role,
                        FamilyId = user.FamilyId
                    }
                });
            }
            catch (Exception ex)
            {
                // Hvis noget går galt (f.eks. database kolonne mismatch), fanger vi det her
                // Dette hjælper med at undgå en "blank" 500 fejl, der blokerer CORS
                return StatusCode(500, new
                {
                    message = "Intern serverfejl under login",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("children")]
        public async Task<IActionResult> GetChildren()
        {
            try
            {
                var children = await _context.Users
                    .Where(u => u.Role == "Child")
                    .Select(u => new { u.Id, u.Username })
                    .ToListAsync();

                return Ok(children);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}