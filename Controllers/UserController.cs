using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;
using BCrypt.Net;

namespace TodoApi.Controllers
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Dette er kodeordet i klartekst fra frontenden
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

                // Vi bruger BCrypt til at verificere det indtastede kodeord mod den hashede version i databasen
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginInfo.PasswordHash, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Forkert brugernavn eller adgangskode" });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
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
                return StatusCode(500, new { message = "Intern serverfejl", error = ex.Message });
            }
        }

        [HttpPost("register-parent")]
        public async Task<IActionResult> RegisterParent([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Brugernavnet er optaget" });

            // Find det næste ledige FamilyId
            int nextFamilyId = 1;
            if (await _context.Users.AnyAsync())
            {
                nextFamilyId = (await _context.Users.MaxAsync(u => u.FamilyId ?? 0)) + 1;
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // HASHER KODEORD
                Role = "Parent",
                FamilyId = nextFamilyId,
                TotalPoints = 0,
                SavingsBalance = 0
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Forælder og familie oprettet!" });
        }

        [HttpPost("register-child")]
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> RegisterChild([FromBody] RegisterRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                var parent = await _context.Users.FindAsync(int.Parse(userIdClaim));
                if (parent == null) return NotFound("Forælder ikke fundet");

                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    return BadRequest(new { message = "Brugernavnet er optaget" });

                var child = new User
                {
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // HASHER KODEORD
                    Role = "Child",
                    FamilyId = parent.FamilyId,
                    TotalPoints = 0,
                    SavingsBalance = 0
                };

                _context.Users.Add(child);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Barn tilføjet til familien!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // --- Hent alle familiemedlemmer (til Dashboard) ---
        [HttpGet("family")]
        [Authorize]
        public async Task<IActionResult> GetFamilyMembers()
        {
            try
            {
                var familyIdClaim = User.FindFirst("FamilyId")?.Value;
                if (string.IsNullOrEmpty(familyIdClaim)) return Unauthorized();

                int familyId = int.Parse(familyIdClaim);

                var members = await _context.Users
                    .Where(u => u.FamilyId == familyId)
                    .Select(u => new {
                        u.Id,
                        u.Username,
                        u.Role,
                        u.TotalPoints
                    })
                    .ToListAsync();

                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("children")]
        public async Task<IActionResult> GetChildren()
        {
            try
            {
                var children = await _context.Users
                    .Where(u => u.Role == "Child")
                    .Select(u => new { u.Id, u.Username, u.TotalPoints })
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