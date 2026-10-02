using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Load variables from .env into the environment before the builder reads configuration.
DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration["DB_URL"]
                 ?? throw new InvalidOperationException("Connection string 'DB_URL' is missing. Copy .env.example to .env and fill it in.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connection));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GameBacklog API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
