using GameBacklog.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            catch (DbUpdateException ex) when (!context.Response.HasStarted && ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.CheckViolation
            } pg)
            {
                if (pg.SqlState == PostgresErrorCodes.UniqueViolation)
                    await WriteProblemAsync(context, StatusCodes.Status409Conflict, "This record already exists.");
                else
                    await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "The request violates a data rule.");
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
