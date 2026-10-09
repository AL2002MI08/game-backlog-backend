using GameBacklog.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GameBacklog.Api.Middleware {
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException ex) when (!context.Response.HasStarted)
            {
                await WriteProblemAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

                if (context.Response.HasStarted) throw;

                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private static Task WriteProblemAsync(HttpContext context, int statusCode, string title)
        {
            context.Response.StatusCode = statusCode;
            var problem = new ProblemDetails { Status = statusCode, Title = title };
            return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}
