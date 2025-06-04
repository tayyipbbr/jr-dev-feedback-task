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
                // appsettings.json'dan MongoDB ayarlar�n� okuyup MongoDbSettings s�n�f�na bind et
                services.Configure<MongoDbSettings>(hostContext.Configuration.GetSection("MongoDb"));

                services.AddMassTransit(x =>
                {
                    // Consumer'� DI container'a ve MassTransit'e kaydet
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

                        // Consumer i�in endpoint'i (kuyru�u) do�rudan isimlendirerek konfig�re et.
                        // RabbitMQ'da olu�an kuyruk ad� bu olacak.
                        // E�er mevcut kuyruk ad�n�z "feedback" ise ve bunu korumak istiyorsan�z, "feedback-queue" yerine "feedback" yaz�n.
                        cfg.ReceiveEndpoint("feedback-queue", e =>
                        {
                            e.ConfigureConsumer<FeedbackConsumer>(context);

                            // Hata y�netimi i�in basit bir retry politikas� (opsiyonel)
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // 3 kez, 5 saniye aral�klarla
                        });
                    });
                });
            });
}
