using GameBacklog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameBacklog.Api.IntegrationTests.Fixtures {
    public class GameBacklogApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            DotNetEnv.Env.TraversePath().Load();
            var testDatabase = Environment.GetEnvironmentVariable("TEST_DB_URL")
                ?? throw new InvalidOperationException("TEST_DB_URL is missing. Add it to .env.");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(testDatabase));
                services.AddSingleton<IStartupFilter, RateLimitBypassStartupFilter>();
            });
        }
    }
}
