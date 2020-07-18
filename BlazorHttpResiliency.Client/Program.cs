using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            }).AddPolicyHandler((sp, msg) => Polly.Policy.WrapAsync(HttpClientPolicies.GetFallbackPolicy(sp, WeatherClient.FallbackValueFactory),
                                                                    HttpClientPolicies.GetRetryPolicy(sp)));

            await builder.Build().RunAsync();
        }
    }
}
