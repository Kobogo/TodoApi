using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;
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
    public async Task<IActionResult> Login([FromBody] User loginInfo)
    {
        // 1. Find brugeren i databasen
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

        // 2. Tjek om brugeren findes og om kodeordet matcher
        // Bemærk: I et rigtigt system ville vi bruge PasswordHashing her
        if (user == null || user.PasswordHash != loginInfo.PasswordHash)
        {
            return Unauthorized(new { message = "Forkert brugernavn eller adgangskode" });
        }

        // 3. Setup JWT Token generering
        var tokenHandler = new JwtSecurityTokenHandler();
        // Vigtigt: Denne nøgle SKAL være identisk med den i Program.cs
        var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? "DIN_MEGET_LANGE_HEMMELIGE_NØGLE_HER_PÅ_MINDST_32_TEGN");

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

        // 4. Returner token og de nødvendige brugeroplysninger til React
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

    [HttpGet("children")]
    public async Task<IActionResult> GetChildren()
    {
        // Henter alle brugere med rollen 'Child'
        var children = await _context.Users
            .Where(u => u.Role == "Child")
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();

        return Ok(children);
    }
}