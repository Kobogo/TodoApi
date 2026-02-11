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
                .Union(context.StaticTasks.Where(t => t.UserId != null).Select(t => t.UserId!.Value))
                .Distinct()
                .ToListAsync();

            var igår = DateTime.Today.AddDays(-1);

            foreach (var userId in userIds)
            {
                // Tæl gennemførte opgaver for dagen der gik
                int dynamicDone = await context.DynamicTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);
                int staticDone = await context.StaticTasks.CountAsync(t => t.UserId == userId && t.IsCompleted);
                int totalDone = dynamicDone + staticDone;

                // Opret logpost
                context.TaskLogs.Add(new TaskLog
                {
                    UserId = userId,
                    Date = igår,
                    TasksCompleted = totalDone,
                    DailyGoal = 3, // Standardmål
                    PointsEarned = totalDone * 10 // 10 point pr. opgave
                });

                _logger.LogInformation("Loggede {Count} opgaver for bruger {UserId}", totalDone, userId);
            }

            // 2. Nulstil statiske opgaver
            var staticTasks = await context.StaticTasks.Where(t => t.IsCompleted).ToListAsync();
            foreach (var st in staticTasks) st.IsCompleted = false;

            // 3. Nulstil dynamiske gentagende opgaver
            var allDynamic = await context.DynamicTasks.Where(t => t.IsCompleted && t.TimeOfDay != null).ToListAsync();
            var repeatingTasks = allDynamic.Where(t => t.RepeatDays != null && t.RepeatDays.Count > 0).ToList();
            foreach (var dt in repeatingTasks) dt.IsCompleted = false;

            await context.SaveChangesAsync();
            _logger.LogInformation("Midnats-nulstilling gennemført.");
        }
    }
}