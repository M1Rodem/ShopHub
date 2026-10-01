using CatalogService.Data;
using CatalogService.Endpoints;
using CatalogService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "CatalogService API",
        Version = "v1",
        Description = "Управление каталогом товаров ShopHub"
    });
});

// ── SQLite через EF Core ──────────────────────────────────────────────────
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── Redis-кэш ─────────────────────────────────────────────────────────────
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "catalog:";
});

// ── Бизнес-сервисы ────────────────────────────────────────────────────────
builder.Services.AddScoped<ProductService>();

// ── Health Checks ─────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddSqlite(
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        name: "sqlite",
        tags: ["db", "ready"])
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: ["cache", "ready"]);

var app = builder.Build();

// ── Swagger UI (dev) ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatalogService v1"));
}

// ── Автомиграции при старте ───────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
}

// ── Эндпоинты ─────────────────────────────────────────────────────────────
app.MapProductEndpoints();
app.MapHealthChecks("/health");
app.MapGet("/", () => "CatalogService is running");

app.Run();