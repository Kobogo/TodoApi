using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Hent connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// --- JWT AUTHENTICATION SETUP ---
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

// --- SIKRER AT DATABASEN OG TABELLER ER OPRETTET (MIGRATION) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        Console.WriteLine("--- Database Initialisering Starter ---");

        // EnsureCreated tvinger PostgreSQL til at lave tabellerne ud fra din AppDbContext
        bool created = context.Database.EnsureCreated();

        if (created) {
            Console.WriteLine("Status: Tabeller er blevet oprettet i Neon PostgreSQL.");
        } else {
            Console.WriteLine("Status: Tabeller findes allerede i databasen.");
        }

        // Hent seed-data fra Render Environment Variables
        var seedUser = Environment.GetEnvironmentVariable("SEED_USER_NAME") ?? "Far";
        var seedPass = Environment.GetEnvironmentVariable("SEED_USER_PASS") ?? "1234";

        if (!context.Users.Any())
        {
            Console.WriteLine($"Status: Ingen brugere fundet. Opretter seed-bruger: {seedUser}");
            context.Users.Add(new User
            {
                Username = seedUser,
                PasswordHash = seedPass,
                Role = "Parent",
                FamilyId = 1
            });
            context.SaveChanges();
            Console.WriteLine("Status: Seed-bruger oprettet succesfuldt.");
        }
        Console.WriteLine("--- Database Initialisering Færdig ---");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "KRITISK FEJL ved database-initialisering!");
        // Vi skriver også til Console så det er nemt at se i Render Logs
        Console.WriteLine($"FEJL: {ex.Message}");
    }
}

// Konfigurer HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger"; // Gør Swagger tilgængelig på /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();