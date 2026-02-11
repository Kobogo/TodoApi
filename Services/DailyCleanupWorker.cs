using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

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
                var dkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var nu = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dkTimeZone);

                // Vi vil køre præcis kl. 00:00:01 hver nat
                var næsteKørsel = nu.Date.AddDays(1);
                var ventetid = næsteKørsel - nu;

                _logger.LogInformation("Næste nulstilling sker om {Ventetid} kl. {Tid}", ventetid, næsteKørsel);

                await Task.Delay(ventetid, stoppingToken);

                try
                {
                    await ResetAndLogTasks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Der skete en fejl under daglig nulstilling af opgaver.");
                }
            }
        }

        private async Task ResetAndLogTasks()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Starter midnats-logning og nulstilling...");

            // 1. Find alle unikke brugere der har opgaver
            var userIds = await context.DynamicTasks.Select(t => t.UserId)
                .Union(context.StaticTasks.Where(t => t.UserId != null).Select(t => (int)t.UserId!))
                .Distinct()
                .ToListAsync();

            // Da vi kører lige efter midnat, logger vi for "i går"
            var dkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var igår = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dkTimeZone).AddDays(-1).Date;

            foreach (var userId in userIds)
            {
                int dynamicDone = await context.DynamicTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);
                int staticDone = await context.StaticTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);
                int totalDone = dynamicDone + staticDone;

                // Gem dagens resultat permanent i historikken
                context.TaskLogs.Add(new TaskLog
                {
                    UserId = userId,
                    Date = igår,
                    TasksCompleted = totalDone,
                    DailyGoal = 3,
                    PointsEarned = totalDone * 10
                });

                _logger.LogInformation("Loggede {Count} opgaver for bruger {UserId}", totalDone, userId);
            }

            // 2. Nulstil statiske opgaver (Alle faste rutiner skal gøres igen i morgen)
            var staticTasks = await context.StaticTasks.Where(t => t.IsCompleted).ToListAsync();
            foreach (var st in staticTasks) st.IsCompleted = false;

            // 3. Nulstil dynamiske gentagende opgaver
            var repeatingTasks = await context.DynamicTasks
                .Where(t => t.IsCompleted && t.RepeatDays != null)
                .ToListAsync();

            // Vi tjekker her kun for dem der faktisk har dage i listen
            foreach (var dt in repeatingTasks)
            {
                if (dt.RepeatDays!.Count > 0)
                {
                    dt.IsCompleted = false;
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Midnats-nulstilling gennemført.");
        }
    }
}