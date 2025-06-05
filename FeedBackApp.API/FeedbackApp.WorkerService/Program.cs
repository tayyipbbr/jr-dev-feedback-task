using FeedbackApp.WorkerService.Consumers;
using FeedbackApp.WorkerService.Models;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FeedbackApp.WorkerService
{
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

                    services.AddSingleton<IMongoClient>(sp =>
                        new MongoClient(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));

                    services.AddScoped(sp =>
                    {
                        var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                        var client = sp.GetRequiredService<IMongoClient>();
                        var database = client.GetDatabase(settings.DatabaseName);
                        return database.GetCollection<FeedbackDocument>(settings.CollectionName);
                    });

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
                            cfg.ReceiveEndpoint("feedback-queue", e =>
                            {
                                e.UseMessageRetry(r =>
                                {
                                    r.Interval(5, TimeSpan.FromSeconds(5));
                                });
                                e.ConfigureConsumer<FeedbackConsumer>(context);
                            });
                        });
                    });
                });
    }
}