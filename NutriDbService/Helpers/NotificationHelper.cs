using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using NutriDbService.DbModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NutriDbService.PythModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NutriDbService.Helpers
{
    public class NotificationHelper
    {

        private readonly static string _api_key = "f3ccf95cf601c3fb7efe18c3b6135d4a";
        private readonly static string _reqUrl = $"https://chatter.salebot.pro/api/{_api_key}/message";
        private readonly static string _diarymess = "36645266";
        private readonly static string _mealmess = "36327038";
        private readonly static string _bothmess = "37023610";
        private railwayContext _context;
        private readonly ILogger _logger;
        
        public NotificationHelper(railwayContext context, IServiceScopeFactory serviceProviderFactory)
        {
            _context = context;
            _logger = serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<NotificationHelper>>();
        }
        private async Task SendNot(long ClientId, string MessBoxId)
        {
            var reqparams = new NocodeNot { client_id = ClientId, message_id = MessBoxId };
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(100);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_reqUrl, content);
            var r = await response.Content.ReadAsStringAsync();
        }
        public async Task SendNotification(int UserId)
        {
            try
            {
                _logger.LogWarning($"User:{UserId} SendNotification");
                bool isMealSend = false;
                bool isDiarySend = false;
                var user = await _context.Users.SingleAsync(x => x.Id == UserId);
                var userInfo = await _context.Userinfos.SingleAsync(x => x.UserId == UserId);
                var meals = await _context.Meals.Where(x => x.UserId == UserId).OrderByDescending(x => x.MealTime).FirstOrDefaultAsync();
                var lastMealTime = meals?.MealTime;
                if (lastMealTime != null && lastMealTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
                    isMealSend = true;
                if (userInfo.LastlessonTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
                    isDiarySend = true;

                if (isMealSend && isDiarySend)
                    await SendNot(user.UserNoId, _bothmess);
                else
                {
                    if (isDiarySend)
                        await SendNot(user.UserNoId, _diarymess);
                    if (isMealSend)
                        await SendNot(user.UserNoId, _mealmess);
                }
                //await SendNot(user.UserNoId, "37023544");
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotificationSendError for User:{UserId}", ex);
                await ErrorHelper.SendErrorMess($"NotificationSendError for User:{UserId}", ex);
            }

        }
    }

    public class NocodeNot
    {
        public string message_id { get; set; }

        public long client_id { get; set; }
    }
}
