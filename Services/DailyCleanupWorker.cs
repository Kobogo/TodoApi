using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Services
{
    public class DailyCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyCleanupWorker> _logger;

        public DailyCleanupWorker(IServiceProvider serviceProvider, ILogger<DailyCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Cleanup Worker starter...");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Beregn tid til næste midnat i dansk tid
                var dkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var nu = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dkTimeZone);
                var næsteKørsel = nu.Date.AddDays(1); // Næste midnat
                var ventetid = næsteKørsel - nu;

                _logger.LogInformation("Næste nulstilling sker om {Ventetid} kl. {Tid}", ventetid, næsteKørsel);

                // Vent indtil midnat (eller indtil servicen stoppes)
                await Task.Delay(ventetid, stoppingToken);

                // 2. Kør nulstillingen
                try
                {
                    await ResetRepeatingTasks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Der skete en fejl under daglig nulstilling af opgaver.");
                }
            }
        }

        private async Task ResetRepeatingTasks()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Starter midnats-nulstilling af opgaver...");

            // 1. Statiske opgaver: Nulstil ALTID hvis de er udført
            var staticTasks = await context.StaticTasks
                .Where(t => t.IsCompleted)
                .ToListAsync();

            foreach (var st in staticTasks) st.IsCompleted = false;

            // 2. Dynamiske opgaver: Nulstil KUN hvis de har både tid og repeatDays
            // Vi henter dem ud i hukommelsen først for at undgå SQL-oversættelsesfejl på RepeatDays.Length
            var allCompletedDynamic = await context.DynamicTasks
                .Where(t => t.IsCompleted && t.TimeOfDay != null)
                .ToListAsync();

            // Filtrer i hukommelsen (C# logik i stedet for SQL logik for arrays)
            var repeatingTasks = allCompletedDynamic
                .Where(t => t.RepeatDays != null && t.RepeatDays.Count > 0)
                .ToList();

            foreach (var dt in repeatingTasks)
            {
                dt.IsCompleted = false;
                _logger.LogInformation("Nulstillede dynamisk opgave: {Title}", dt.Title);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Midnats-nulstilling gennemført succesfuldt.");
        }
    }
}