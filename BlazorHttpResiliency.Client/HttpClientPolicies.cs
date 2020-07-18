using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Threading.Tasks;
using System.Threading;

namespace BlazorHttpResiliency.Client
{
    public static class HttpClientPolicies
    {
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
        private static readonly Random _jitterer = new Random();

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, int retryCount = 3) =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(_jitterer.Next(0, 100)),
                    onRetry: (result, span, index, ctx) =>
                    {
                        var logger = serviceProvider.GetService<ILogger<WeatherClient>>();
                        logger.LogWarning($"tentative #{index}, received {result.Result.StatusCode}, retrying...");
                    });

        public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(IServiceProvider serviceProvider, Func<Context, CancellationToken, Task<HttpResponseMessage>> valueFactory) =>
            HttpPolicyExtensions
            .HandleTransientHttpError()
            .FallbackAsync(valueFactory,
                (res, ctx) =>
                {
                    var logger = serviceProvider.GetService<ILogger<WeatherClient>>();
                    logger.LogWarning($"returning fallback value...");
                    return Task.CompletedTask;
                });
    }
}
