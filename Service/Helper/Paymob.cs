using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Service.Helper
{
    public class Paymob
    {
        public record BillingData(
        string first_name, string last_name,
        string email, string phone_number);

        public record PaymobItem(
            string name, int amount,string description, int quantity);

        public record IntentionRequest(
            int amount,
            string currency,
            List<int> payment_methods,
            List<PaymobItem> items,
            BillingData billing_data,
            string notification_url,
            string redirection_url,
            Dictionary<string, string> extras);

        public record IntentionResponse(
            string client_secret,
            string id);

        public class PaymobClient
        {
            private readonly HttpClient _httpClient;
            private readonly PaymobSettings _settings;

            public PaymobClient(HttpClient httpClient, IOptions<PaymobSettings> options)
            {
                _httpClient = httpClient;
                _settings = options.Value;

                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Token", _settings.SecretKey);
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }

            public async Task<IntentionResponse> CreateIntentionAsync(
                IntentionRequest request)
            {
                var json = JsonSerializer.Serialize(request,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Paymob v2 intention endpoint
                var response = await _httpClient.PostAsync("https://accept.paymob.com/v1/intention", content);

                var body = await response.Content.ReadAsStringAsync();
                //if (!response.IsSuccessStatusCode)
                //{
                //    var errorJson = await response.Content.ReadAsStringAsync();
                //    // Set a breakpoint here and inspect errorJson
                //    Console.WriteLine(errorJson);
                //}
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Paymob error: {body}");

                return JsonSerializer.Deserialize<IntentionResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
        }
    }
}
