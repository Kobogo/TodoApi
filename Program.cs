using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// --- JWT AUTHENTICATION SETUP ---
var key = Encoding.ASCII.GetBytes("DIN_MEGET_LANGE_HEMMELIGE_NØGLE_HER_PÅ_MINDST_32_TEGN");
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

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication(); // Vigtigt: Skal stå før Authorization
app.UseAuthorization();

app.MapControllers();
// --- SIKRER AT DATABASEN OG TABELLER ER OPRETTET ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Denne linje kigger på din AppDbContext og opretter alt der mangler i Neon
    context.Database.EnsureCreated();

    // Valgfrit: Opret "Far" hvis han ikke findes, så du kan logge ind med det samme
    if (!context.Users.Any())
    {
        context.Users.Add(new TodoApi.Models.User
        {
            Username = "Far",
            PasswordHash = "1234",
            Role = "Parent",
            FamilyId = 1
        });
        context.SaveChanges();
    }
}
app.Run();