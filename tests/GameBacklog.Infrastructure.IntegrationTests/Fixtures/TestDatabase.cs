using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameBacklog.Infrastructure.IntegrationTests.Fixtures;

public static class TestDatabase
{
    public static AppDbContext CreateContext()
    {
        DotNetEnv.Env.TraversePath().Load();
        var connection = Environment.GetEnvironmentVariable("TEST_DB_URL")
            ?? throw new InvalidOperationException("TEST_DB_URL is missing. Add it to .env (see .env.example).");

        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options);
    }

    public static Guid AddUser(AppDbContext context)
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = $"{userId}@example.com",
            PasswordHash = "not-used",
            CreatedAt = DateTime.UtcNow,
        });
        context.SaveChanges();
        return userId;
    }

    public static void DeleteUser(AppDbContext context, Guid userId)
    {
        context.ChangeTracker.Clear();
        context.Users.Where(u => u.Id == userId).ExecuteDelete();
        context.Dispose();
    }

    public static Game NewGame(Guid userId, string title = "Elden Ring", Platform platform = Platform.STEAM) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = title,
        Platform = platform,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public static Objective NewObjective(Guid gameId, string label = "Beat Margit") => new()
    {
        Id = Guid.NewGuid(),
        GameId = gameId,
        Label = label,
        Position = 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public static async Task<string> SaveAndGetErrorCodeAsync(AppDbContext context)
    {
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        return Assert.IsType<PostgresException>(ex.InnerException).SqlState;
    }
}
