using MassTransit;
using FeedbackApp.WorkerService.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // appsettings.json'dan MongoDB ayarlarýný okuyup MongoDbSettings sýnýfýna bind et
                services.Configure<MongoDbSettings>(hostContext.Configuration.GetSection("MongoDb"));

                services.AddMassTransit(x =>
                {
                    // Consumer'ý DI container'a ve MassTransit'e kaydet
                    x.AddConsumer<FeedbackConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        var rabbitMqConfig = hostContext.Configuration.GetSection("RabbitMq");
                        var host = rabbitMqConfig["Host"] ?? "localhost";
                        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
                        var username = rabbitMqConfig["Username"] ?? "guest";
                        var password = rabbitMqConfig["Password"] ?? "guest";

                        cfg.Host(host, virtualHost, h =>
                        {
                            h.Username(username);
                            h.Password(password);
                        });

                        // Consumer için endpoint'i (kuyruðu) doðrudan isimlendirerek konfigüre et.
                        // RabbitMQ'da oluþan kuyruk adý bu olacak.
                        // Eðer mevcut kuyruk adýnýz "feedback" ise ve bunu korumak istiyorsanýz, "feedback-queue" yerine "feedback" yazýn.
                        cfg.ReceiveEndpoint("feedback-queue", e =>
                        {
                            e.ConfigureConsumer<FeedbackConsumer>(context);

                            // Hata yönetimi için basit bir retry politikasý (opsiyonel)
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // 3 kez, 5 saniye aralýklarla
                        });
                    });
                });
            });
}
