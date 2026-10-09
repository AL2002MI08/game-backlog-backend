using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace GameBacklog.Api.IntegrationTests.Fixtures {

   // Assign a unique fake IP per request prevents 429 Too Many Requests in tests.
    public class RateLimitBypassStartupFilter : IStartupFilter
    {
        private static int _requestCount;

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                var n = Interlocked.Increment(ref _requestCount);
                context.Connection.RemoteIpAddress = new IPAddress([10, (byte)(n >> 16), (byte)(n >> 8), (byte)n]);
                await nextMiddleware();
            });
            next(app);
        };
    }
}
