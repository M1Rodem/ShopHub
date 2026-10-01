using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Endpoints;
using OrderService.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ── Swagger ───────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "OrderService API",
        Version = "v1",
        Description = "Оформление заказов ShopHub. Вызывает CatalogService по HTTP и публикует события в RabbitMQ."
    });
});

// ── EF Core + SQLite ──────────────────────────────────────────────────
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── HTTP-клиент CatalogService + Polly (retry + circuit breaker) ──────
builder.Services
    .AddHttpClient<ICatalogClient, CatalogClient>(c =>
    {
        c.BaseAddress = new Uri(
            builder.Configuration["Services:Catalog"] ?? "http://catalog-service:8080");
        c.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry))))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)));

// ── MassTransit + RabbitMQ ────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
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
    });
});

// ── Health Checks ─────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlite(
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        name: "sqlite",
        tags: new[] { "db", "ready" });

var app = builder.Build();

// ── Swagger UI (dev) ──────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1"));
}

// ── Автомиграции при старте ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

// ── Эндпоинты ─────────────────────────────────────────────────────────
app.MapOrderEndpoints();
app.MapHealthChecks("/health");
app.MapGet("/", () => "OrderService is running");

app.Run();