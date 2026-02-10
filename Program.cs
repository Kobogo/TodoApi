using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text;
using TodoApi.Data;
using TodoApi.Services;
using TodoApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE SETUP (Uændret)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "TodoApi");
    });
});

// 2. JWT AUTHENTICATION SETUP med Cookie Support
var jwtSecret = Environment.GetEnvironmentVariable("JWT_KEY") ?? "DIN_MEGET_LANGE_HEMMELIGE_NØGLE_HER_PÅ_MINDST_32_TEGN";
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
        ValidateAudience = false,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
    };

    // --- NYT: Her fortæller vi API'et at det skal kigge i cookies ---
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1. Tjek først Cookies
            var accessToken = context.Request.Cookies["AuthToken"];

            // 2. Hvis ingen cookie, tjek URL Query (f.eks. ?token=...)
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Query["token"];
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 3. CORS SETUP - VIGTIGT for cookies!
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://todorewardsapp.netlify.app", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // <--- SKAL bruges når man sender cookies/tokens
    });
});

// ... resten af dine services (Controllers, Swagger, etc.) er uændret ...
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHostedService<TaskNotificationWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ... resten af din pipeline (Swagger, Migration, Https, CORS, Auth) ...
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger";
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var config = services.GetRequiredService<IConfiguration>();
    context.Database.Migrate();
    TodoApi.Data.DbInitializer.Seed(context, config);
}

app.UseHttpsRedirection();

// Husk: CORS skal ligge før Authentication
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();