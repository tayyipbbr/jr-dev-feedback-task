using MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? "localhost"; // default bilgiler """
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/"; 
        var username = rabbitMqConfig["Username"] ?? "guest";
        var password = rabbitMqConfig["Password"] ?? "guest";

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyReactApp", 
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:5173") // feedback-frontend
                          .AllowAnyHeader()
                          .AllowAnyMethod();
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

app.UseCors("AllowMyReactApp"); // konum önemli, auth öncesi cors kontrolü var.

app.UseAuthorization();

app.MapControllers();

app.Run();
