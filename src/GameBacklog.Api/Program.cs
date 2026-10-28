using System.Text.Json.Serialization;
using GameBacklog.Api.Events;
using GameBacklog.Api.Extensions;
using GameBacklog.Api.Middleware;
using GameBacklog.Api.OpenApi;
using GameBacklog.Api.Services;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection)) connection = builder.Configuration["DB_URL"];
if (string.IsNullOrWhiteSpace(connection))
    throw new InvalidOperationException("Database connection is missing. Set ConnectionStrings:DefaultConnection, or DB_URL in .env (see .env.example).");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connection));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(tags: ["ready"]);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IEventPublisher, LoggingEventPublisher>();

var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddAuthRateLimiting();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});

var app = builder.Build();

// The in-cluster database starts empty; Kubernetes sets RUN_MIGRATIONS=true so the schema is created on startup.
if (app.Configuration.GetValue<bool>("RUN_MIGRATIONS"))
{
    await app.ApplyMigrationsAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GameBacklog API");
    });
}

// Plain-HTTP hosts (the desktop client's local process, the Docker image) set USE_HTTPS_REDIRECTION=false.
if (app.Configuration.GetValue("USE_HTTPS_REDIRECTION", true))
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();
