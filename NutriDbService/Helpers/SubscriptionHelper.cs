using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NutriDbService.DbModels;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using NutriDbService.PayModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NutriDbService.Helpers
{

    public class SubscriptionHelper
    {
        private readonly static string _api_key = "a87df4c5cb5c6c8cc36ad24c8db5e54b";
        private readonly static string _succmess = "38327266";
        public railwayContext _nutriDbContext { get; set; }
        private readonly IServiceScopeFactory _serviceProviderFactory;
        private readonly ILogger _logger;
        public SubscriptionHelper(railwayContext nutriDbContext, IServiceScopeFactory serviceProviderFactory)
        {
            _nutriDbContext = nutriDbContext;
            _serviceProviderFactory = serviceProviderFactory;
            _logger = _serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<SubscriptionHelper>>();
        }

        public async Task<bool> CheckSub(long TgId)
        {
            var user = await _nutriDbContext.Users.SingleOrDefaultAsync(x => x.TgId == TgId);
            return user?.IsActive == true;
        }
        private string ConvertRequestToJSON(string input)
        {
            var entries = input.Split('&');
            var transactionDict = new Dictionary<string, object>();

            foreach (var entry in entries)
            {
                var keyValue = entry.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = Uri.UnescapeDataString(keyValue[1]);

                    if (key == "Data" || key == "CustomFields")
                    {
                        transactionDict.Add(key, JsonConvert.DeserializeObject(value));
                    }
                    else
                    {
                        transactionDict.Add(key, value);
                    }
                }
            }

            // Serialize dictionary to JSON format
            return JsonConvert.SerializeObject(transactionDict, Formatting.Indented);

        }
        public SuccessPayRequest ConvertToPayRequestJSON(string input)
        {
            var json = ConvertRequestToJSON(input);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SuccessPayRequest>(json);
        }
        public SuccessInfoPayRequest ConvertToInfoPayRequestJSON(string input)
        {
            var json = ConvertRequestToJSON(input);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SuccessInfoPayRequest>(json);
        }
        public FailPayRequest ConvertToFailRequestJSON(string input)
        {
            var json = ConvertRequestToJSON(input);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FailPayRequest>(json);
        }
        public RecurrentRequest ConvertToReqRequestJSON(string input)
        {
            var json = ConvertRequestToJSON(input);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<RecurrentRequest>(json);
        }

        public async Task<bool> SendPayNoti(long userTgId)
        {
            try
            {
                var noId = await GetUserNoId(userTgId);
                await SendSuccNot(noId, _succmess);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task SendSuccNot(long ClientNoId, string MessBoxId)
        {
            var reqparams = new NocodeNot { client_id = ClientNoId, message_id = MessBoxId };
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(100);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://chatter.salebot.pro/api/{_api_key}/message", content);
            var r = await response.Content.ReadAsStringAsync();
        }
        private async Task<long> GetUserNoId(long userTgId)
        {
            var reqparams = new NocodeGet { platform_ids = new List<string> { $"{userTgId}" }, group_id = "nutri_pay_bot" };
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(100);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "text/plain");
            var response = await client.PostAsync($"https://chatter.salebot.pro/api/{_api_key}/find_client_id_by_platform_id", content);
            var r = await response.Content.ReadAsStringAsync();
            JArray jsonArray = JArray.Parse(r);

            // Извлекаем первый объект из массива
            JObject jsonObject = (JObject)jsonArray[0];

            // Получаем значение свойства "id"
            long id = jsonObject.Value<long>("id");
            return id;
        }
        public class NocodeGet
        {
            public List<string> platform_ids { get; set; }

            public string group_id { get; set; }
        }
    }
}
