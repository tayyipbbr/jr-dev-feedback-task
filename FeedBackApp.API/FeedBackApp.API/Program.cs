using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // Ýleride consumer'larýnýz API projesinde olacaksa bu satýrlarý açabilirsiniz.
    // x.SetKebabCaseEndpointNameFormatter(); // Kuyruk isimlerini kebab-case formatýnda oluþturur (örn: feedback-received)
    // x.AddConsumers(Assembly.GetExecutingAssembly()); // Bu assembly'deki consumer'larý tarar

    x.UsingRabbitMq((context, cfg) =>
    {
        // RabbitMQ baðlantý bilgilerini appsettings.json'dan alalým
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? "localhost"; // Varsayýlan olarak localhost
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/"; // Varsayýlan olarak kök virtual host
        var username = rabbitMqConfig["Username"] ?? "guest"; // Varsayýlan kullanýcý
        var password = rabbitMqConfig["Password"] ?? "guest"; // Varsayýlan þifre

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Consumer'larýnýz farklý bir projede ise veya endpoint'leri manuel konfigüre etmek isterseniz:
        // cfg.ConfigureEndpoints(context);
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
