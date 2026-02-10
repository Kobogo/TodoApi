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
                var nuDateTime = DateTime.Now;
                // Vi runder ned til hele minutter for at matche databasen
                var nuTimeSpan = new TimeSpan(nuDateTime.Hour, nuDateTime.Minute, 0);

                _logger.LogInformation("Worker tjekker databasen for tidspunkt: {time}", nuTimeSpan);

                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var staticTasks = db.StaticTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted && t.UserId != null)
                        .Select(t => new { t.Id, UserId = t.UserId.Value, t.Title })
                        .ToList();

                    var dynamicTasks = db.DynamicTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted)
                        .Select(t => new { t.Id, UserId = t.UserId, t.Title })
                        .ToList();

                    var alleOpgaver = staticTasks.Select(t => new { t.Id, t.UserId, t.Title })
                        .Concat(dynamicTasks.Select(t => new { t.Id, t.UserId, t.Title }))
                        .ToList();

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

                // --- SMART TIMING LOGIK HER ---
                // Find ud af hvor mange sekunder der er til det næste hele minut
                var nu = DateTime.Now;
                var næsteMinut = nu.AddMinutes(1).AddSeconds(-nu.Second).AddMilliseconds(-nu.Millisecond);
                var ventetid = næsteMinut - nu;

                _logger.LogInformation("Venter {seconds} sekunder til næste tjek...", ventetid.TotalSeconds);

                await Task.Delay(ventetid, stoppingToken);
            }
        }

        private async Task SendPush(PushSubscriptionEntity sub, string taskTitle, int taskId)
        {
            var payload = JsonSerializer.Serialize(new {
                title = "Tid til opgave! 🏆",
                body = taskTitle,
                data = new { taskId = taskId }
            });

            // 1. Opret subscription objektet fra databasen
            var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);

            // 2. Hent VAPID detaljer fra IConfiguration
            // VIGTIGT: Vi bruger "VapidDetails:..." fordi det er navnet i din appsettings.json
            var vapidDetails = new VapidDetails(
                _config["VapidDetails:subject"],
                _config["VapidDetails:publicKey"],
                _config["VapidDetails:privateKey"]
            );

            var client = new WebPushClient();
            try
            {
                // 3. Send notifikationen
                await client.SendNotificationAsync(pushSub, payload, vapidDetails);
                Console.WriteLine($"✅ Push sendt til bruger: {sub.UserId}");
            }
            catch (WebPushException ex)
            {
                // 410 Gone betyder at token ikke længere er gyldig (brugeren har blokeret eller slettet app)
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                {
                    Console.WriteLine($"🚫 Subscription er udløbet for bruger {sub.UserId}. Bør slettes.");
                    // Her kunne du tilføje logik til at fjerne sub fra databasen
                }
                else
                {
                    Console.WriteLine($"❌ Push fejlede med status: {ex.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Generel fejl ved push: {ex.Message}");
            }
        }
    }
}