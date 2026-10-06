using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.UnitTests.Fixtures;

public static class TestDatabase
{
    public static AppDbContext CreateContext()
    {
        DotNetEnv.Env.TraversePath().Load();
        var connection = Environment.GetEnvironmentVariable("TEST_DB_URL")
            ?? throw new InvalidOperationException("TEST_DB_URL is missing. Add it to .env (see .env.example).");

        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options);
    }
}
