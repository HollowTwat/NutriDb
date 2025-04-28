using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using NutriDbService.IntegratorModels;
using NutriDbService.DbModels;

namespace NutriDbService.Helpers
{
    public class IntegratorHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://tglk.ru/in/4wKjJIPQnfZzqyQ4";
        public IntegratorHelper()
        {
            _httpClient = new HttpClient();
        }
        public async Task<IntegratorResponse> SendRequestAsync<TRequest>(TRequest request) where TRequest : class
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var jsonContent = JsonSerializer.Serialize(request, options);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, httpContent);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IntegratorResponse>(responseJson, options);
            }

            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess($"Упали при отправке Сообщения интегратору", ex);
                return new IntegratorResponse { Status = false };
            }
        }
    }
}
