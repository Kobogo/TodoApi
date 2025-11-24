using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 1) Læs Postgres connection string
// -------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// -------------------------------------------------------
// 2) Registrér DbContext med PostgreSQL (Neon)
// -------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// -------------------------------------------------------
// 3) Services
// -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------
// 4) Pipeline
// -------------------------------------------------------
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
