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

            var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails(
                "mailto:din@email.dk",
                _config["Vapid:PublicKey"],
                _config["Vapid:PrivateKey"]
            );

            var client = new WebPushClient();
            try {
                await client.SendNotificationAsync(pushSub, payload, vapidDetails);
            } catch (WebPushException ex) {
                // Hvis browseren siger "410 Gone", betyder det brugeren har afmeldt sig.
                // Her bør du slette 'sub' fra databasen.
                Console.WriteLine($"Push fejlede: {ex.StatusCode}");
            }
        }
    }
}