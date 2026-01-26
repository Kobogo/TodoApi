using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE SETUP
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// 2. JWT AUTHENTICATION SETUP
// Vi bruger den samme nøgle som i UserController til at validere indkommende tokens
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
        ValidateAudience = false,
        // VIGTIGT: Fortæller systemet at det skal kigge efter roller i det korrekte Claim
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

// 3. CORS SETUP
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

// 4. CONTROLLERS & JSON SETUP
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. MIDDLEWARE PIPELINE
// Swagger skal altid ligge øverst så det er tilgængeligt
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// CORS skal ligge før Authentication
app.UseCors("AllowAll");

// Authentication tjekker HVEM du er (Token validering)
app.UseAuthentication();

// Authorization tjekker HVAD du må (Rolle tjek f.eks. "Parent")
app.UseAuthorization();

app.MapControllers();

app.Run();