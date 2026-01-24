using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

var jwtSecret = Environment.GetEnvironmentVariable("JWT_KEY") ?? "EnMegetLangFallbackNoegleSomKunBrugesLokalt123!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("--- Database Check Starter ---");

        // Da du har lavet tabellerne manuelt, vil denne linje blot konstatere de findes
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var seedUser = Environment.GetEnvironmentVariable("SEED_USER_NAME") ?? "Far";
            var seedPass = Environment.GetEnvironmentVariable("SEED_USER_PASS") ?? "1234";

            context.Users.Add(new User
            {
                Username = seedUser,
                PasswordHash = seedPass,
                Role = "Parent",
                FamilyId = 1
            });
            context.SaveChanges();
            Console.WriteLine("Seed-bruger oprettet.");
        }
        Console.WriteLine("--- Database Check Færdig ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FEJL ved opstart: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();