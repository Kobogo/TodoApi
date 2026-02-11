using WebPush;
using System.Text.Json;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services{
    public class TaskNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<TaskNotificationWorker> _logger;

        public TaskNotificationWorker(IServiceProvider services, IConfiguration config, ILogger<TaskNotificationWorker> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        // ... (toppen af filen er uændret)

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Hent den aktuelle tid i Dansk tid (ligesom i controlleren)
                var info = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var dkNu = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, info);

                // 2. Rund ned til hele minutter
                var nuTimeSpan = new TimeSpan(dkNu.Hour, dkNu.Minute, 0);

                _logger.LogInformation("Worker tjekker databasen for DANSK tid: {time}", nuTimeSpan);

                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Hent opgaver der matcher den danske tid
                    var staticTasks = db.StaticTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted && t.UserId != null)
                        .Select(t => new { t.Id, UserId = t.UserId ?? 0, t.Title })
                        .ToList();

                    var dynamicTasks = db.DynamicTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted)
                        .Select(t => new { t.Id, t.UserId, t.Title })
                        .ToList();

                    var alleOpgaver = staticTasks.Concat(dynamicTasks).ToList();

                    foreach (var opgave in alleOpgaver)
                    {
                        var subs = db.PushSubscriptions
                            .Where(s => s.UserId == opgave.UserId)
                            .ToList();

                        foreach (var sub in subs)
                        {
                            await SendPush(sub, opgave.Title, opgave.Id);
                        }
                    }
                }

                // --- SMART TIMING (Brug dkNu her også for at beregne ventetid præcist) ---
                var næsteMinut = dkNu.AddMinutes(1).AddSeconds(-dkNu.Second).AddMilliseconds(-dkNu.Millisecond);
                var ventetid = næsteMinut - dkNu;

                _logger.LogInformation("Venter {seconds} sekunder til næste tjek...", ventetid.TotalSeconds);

                await Task.Delay(ventetid, stoppingToken);
            }
        }

        private async Task SendPush(PushSubscriptionEntity sub, string taskTitle, int taskId)
        {
            var payload = JsonSerializer.Serialize(new {
                title = "Tid til opgave! 🏆",
                body = taskTitle,
                data = new { taskId = taskId },
                actions = new[] {
                new { action = "complete", title = "Godkend ✅" },
                new { action = "snooze", title = "Snooze 😴" }
            }
            });

            var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);

            var vapidDetails = new VapidDetails(
                _config["VapidDetails:subject"],
                _config["VapidDetails:publicKey"],
                _config["VapidDetails:privateKey"]
            );

            var client = new WebPushClient();
            try
            {
                await client.SendNotificationAsync(pushSub, payload, vapidDetails);
                _logger.LogInformation("✅ Push sendt til bruger: {UserId}", sub.UserId);
            }
            catch (WebPushException ex)
            {
                // 410 Gone eller 404 Not Found betyder begge at push-token ikke længere er gyldig
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("🚫 Subscription er udløbet for bruger {UserId}. Sletter fra database...", sub.UserId);

                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // Vi finder entiteten igen i denne nye context for at kunne slette den
                        var toDelete = await db.PushSubscriptions.FindAsync(sub.Id);
                        if (toDelete != null)
                        {
                            db.PushSubscriptions.Remove(toDelete);
                            await db.SaveChangesAsync();
                            _logger.LogInformation("🗑️ Udløbet subscription fjernet.");
                        }
                    }
                }
                else
                {
                    _logger.LogError("❌ Push fejlede med status: {Status}", ex.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("⚠️ Generel fejl ved push: {Message}", ex.Message);
            }
        }
    }
}