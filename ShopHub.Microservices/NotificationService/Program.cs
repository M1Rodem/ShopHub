using MassTransit;
using NotificationService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// ── MassTransit + RabbitMQ ────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        // Явная очередь для потребителя событий о заказах
        cfg.ReceiveEndpoint("order-created-notifications", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(ctx);
        });
    });
});

// ── Health Checks ─────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => "NotificationService is running");

app.Run();