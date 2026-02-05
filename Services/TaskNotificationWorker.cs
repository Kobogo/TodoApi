using WebPush;
using System.Text.Json;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services{
    public class TaskNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public TaskNotificationWorker(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Lav det nuværende tidspunkt om til en TimeSpan (f.eks. 14:30:00)
                // Vi fjerner sekunderne, så vi matcher præcis på minuttet.
                var nuDateTime = DateTime.Now;
                var nuTimeSpan = new TimeSpan(nuDateTime.Hour, nuDateTime.Minute, 0);

                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 2. Find statiske opgaver
                    var staticTasks = db.StaticTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted)
                        .Select(t => new { UserId = (int)t.UserId, t.Title }) // Tving til int
                        .ToList();

                    // 3. Find dynamiske opgaver
                    var dynamicTasks = db.DynamicTasks
                        .Where(t => t.TimeOfDay == nuTimeSpan && !t.IsCompleted)
                        .Select(t => new { UserId = (int)t.UserId, t.Title }) // Tving til int
                        .ToList();

                    // Nu er begge lister af typen <int, string>, og Concat virker!
                    var alleOpgaver = staticTasks.Concat(dynamicTasks).ToList();

                    foreach (var opgave in alleOpgaver)
                    {
                        var userIdString = opgave.UserId.ToString();
                        var subs = db.PushSubscriptions
                            .Where(s => s.UserId == userIdString)
                            .ToList();

                        foreach (var sub in subs)
                        {
                            await SendPush(sub, opgave.Title);
                        }
                    }
                }

                // Vent 60 sekunder
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task SendPush(PushSubscriptionEntity sub, string taskTitle)
        {
            var payload = JsonSerializer.Serialize(new {
                title = "Tid til opgave! 🏆",
                body = taskTitle
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