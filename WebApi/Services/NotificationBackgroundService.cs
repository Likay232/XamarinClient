using Cronos;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using Task = System.Threading.Tasks.Task;

namespace WebApi.Services;

public class NotificationBackgroundService(DataComponent component) : BackgroundService
{
    private readonly CronExpression _cronExpression = CronExpression.Parse("0 15 * * *");
    private static bool Initialized { get; set; }

    private static void InitializeNotifications()
    {
        var rootPath = AppContext.BaseDirectory;
        
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile($"{rootPath}/firebase-credentials.json")
        });
        
        Initialized = true;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Initialized)
            InitializeNotifications();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _cronExpression.GetNextOccurrence(DateTime.UtcNow);

            if (!next.HasValue) continue;
            
            var delay = next - DateTime.Now;
            
            if (delay.Value.TotalMilliseconds > 0)
                await Task.Delay(delay.Value, stoppingToken);

            await SendNotifications();
        }
    }

    private async Task SendNotifications()
    {
        var currentDate = DateTime.UtcNow;
        
        var deviceTokens = component.UserDevices
            .Include(d => d.User)
            .Where(ud => (currentDate - ud.User.LastLogin).TotalDays >= 3)
            .Select(ud => ud.DeviceToken)
            .Distinct()
            .ToList();
        
        var message = new MulticastMessage()
        {
            Data = new Dictionary<string, string>
            {
                { "title", "Вы давно не заходили!" },
                { "body", "Я конечно не зеленая сова, но я с ней знаком. Ты же любишь свою семью?" },
            },
            Tokens = deviceTokens,
        };
        
        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        
        Console.WriteLine($"{response.SuccessCount} сообщений было отправлено успешно.");
        
        if (response.FailureCount > 0)
        {
            foreach (var resp in response.Responses)
            {
                if (!resp.IsSuccess)
                {
                    Console.WriteLine($"Ошибка при отправке сообщения: {resp.Exception.Message}");
                }
            }
        }
    }
}