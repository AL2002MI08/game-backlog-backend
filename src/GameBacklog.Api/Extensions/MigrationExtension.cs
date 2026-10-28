using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameBacklog.Api.Extensions {
    public static class MigrationExtension {
        public static async Task ApplyMigrationsAsync(this WebApplication app, int maxAttempts = 10)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await db.Database.MigrateAsync();
                    return;
                }
                catch (NpgsqlException ex) when (attempt < maxAttempts)
                {
                    app.Logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{MaxAttempts}), retrying in 3s", attempt, maxAttempts);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
