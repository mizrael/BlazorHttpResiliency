using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Text;

namespace BlazorHttpResiliency.Client
{
    public class WeatherClient
    {
        private readonly HttpClient _httpClient;

        public WeatherClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeather(bool forceFail)
        {
            var data = await _httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>($"/weatherforecast?forceFail={forceFail}");
            return data;
        }

        public static Task<HttpResponseMessage> FallbackValueFactory(Polly.Context context, CancellationToken cancellationToken)
        {
            var items = Enumerable.Empty<WeatherForecast>();
            var json = System.Text.Json.JsonSerializer.Serialize(items);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")                
            };

            return Task.FromResult(response);
        }
    }
}