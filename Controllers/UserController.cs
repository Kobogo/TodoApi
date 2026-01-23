using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Data;
using ToDoAPI.Models;

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
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginInfo.Username);

        // Simpel tjek (I produktion bør du bruge BCrypt til at tjekke PasswordHash)
        if (user == null || user.PasswordHash != loginInfo.PasswordHash)
            return Unauthorized("Forkert brugernavn eller adgangskode");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("DIN_MEGET_LANGE_HEMMELIGE_NØGLE_HER_PÅ_MINDST_32_TEGN");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FamilyId", user.FamilyId?.ToString() ?? "0")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new {
            Token = tokenHandler.WriteToken(token),
            User = new { user.Id, user.Username, user.Role, user.FamilyId }
        });
    }

    [HttpGet("children")]
    public async Task<IActionResult> GetChildren()
    {
        var children = await _context.Users
            .Where(u => u.Role == "Child")
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();
        return Ok(children);
    }
}