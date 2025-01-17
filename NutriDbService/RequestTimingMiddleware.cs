using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NutriDbService
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            context.Response.OnStarting(() =>
            {
                stopwatch.Stop();
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    string methodName = endpoint.DisplayName;
                    _logger.LogInformation("{MethodName} executed in {ElapsedMilliseconds} ms", methodName, stopwatch.ElapsedMilliseconds);
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    public static class RequestTimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTimingMiddleware>();
        }
    }
}
