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

namespace NutriDbService.Helpers
{

    public class SubscriptionHelper
    {
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
        public FailPayRequest ConvertToFailRequestJSON(string input)
        {
            var json = ConvertRequestToJSON(input);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FailPayRequest>(json);
        }
    }
}
