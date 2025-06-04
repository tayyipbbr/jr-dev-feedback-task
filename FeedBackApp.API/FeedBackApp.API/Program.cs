using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // �leride consumer'lar�n�z API projesinde olacaksa bu sat�rlar� a�abilirsiniz.
    // x.SetKebabCaseEndpointNameFormatter(); // Kuyruk isimlerini kebab-case format�nda olu�turur (�rn: feedback-received)
    // x.AddConsumers(Assembly.GetExecutingAssembly()); // Bu assembly'deki consumer'lar� tarar

    x.UsingRabbitMq((context, cfg) =>
    {
        // RabbitMQ ba�lant� bilgilerini appsettings.json'dan alal�m
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? "localhost"; // Varsay�lan olarak localhost
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/"; // Varsay�lan olarak k�k virtual host
        var username = rabbitMqConfig["Username"] ?? "guest"; // Varsay�lan kullan�c�
        var password = rabbitMqConfig["Password"] ?? "guest"; // Varsay�lan �ifre

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Consumer'lar�n�z farkl� bir projede ise veya endpoint'leri manuel konfig�re etmek isterseniz:
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
