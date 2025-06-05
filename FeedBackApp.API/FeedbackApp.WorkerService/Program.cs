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
                services.Configure<MongoDbSettings>(hostContext.Configuration.GetSection("MongoDb"));

                services.AddMassTransit(x =>
                {
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

                        // rabbitmq kuyruk adý
                        cfg.ReceiveEndpoint("feedback-queue", e =>
                        {
                            e.ConfigureConsumer<FeedbackConsumer>(context);

                            // 3 kez, 5 saniye aralýklarla TODO: hata durumund ekstra kontrol bakýlacak ???
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); 
                        });
                    });
                });
            });
}
