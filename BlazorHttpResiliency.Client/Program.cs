using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace BlazorHttpResiliency.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddHttpClient<WeatherClient>((sp, client) =>
            {
                client.BaseAddress = new Uri(builder.Configuration["weatherApi"]);
            }).AddPolicyHandler((sp, msg) => HttpClientPolicies.GetRetryPolicy(sp));

            await builder.Build().RunAsync();
        }
    }

    public static class HttpClientPolicies
    {
        private static readonly HashSet<System.Net.HttpStatusCode> RetryableCodes = new HashSet<HttpStatusCode>()
        {
            System.Net.HttpStatusCode.InternalServerError,
            System.Net.HttpStatusCode.ServiceUnavailable,
        };

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, int retryCount = 3)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => RetryableCodes.Contains(msg.StatusCode))
                .WaitAndRetryAsync(retryCount, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                    onRetry: (result, span, index, ctx) =>
                    {
                        var logger = serviceProvider.GetService<ILogger<WeatherClient>>(); 
                        logger.LogWarning($"tentative #{index}, received {result.Result.StatusCode}, retrying...");
                    });
        }
    }
}
