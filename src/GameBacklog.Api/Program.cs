using System.Text.Json.Serialization;
using GameBacklog.Api.Extensions;
using GameBacklog.Api.Middleware;
using GameBacklog.Api.OpenApi;
using GameBacklog.Api.Services;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration["DB_URL"]
                 ?? throw new InvalidOperationException("Connection string 'DB_URL' is missing. Copy .env.example to .env and fill it in.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connection));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddAuthRateLimiting();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GameBacklog API");
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
