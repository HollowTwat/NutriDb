using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using NutriDbService.DbModels;
using System.Linq;

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
        public NotificationHelper(railwayContext context)
        {
            _context = context;
        }
        public async Task SendNotification(long ClientId,string MessBoxId)
        {
            var reqparams = new NocodeNot { client_id = ClientId, message_id = MessBoxId };
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(100);
            HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_reqUrl, content);
            var r = await response.Content.ReadAsStringAsync();
        }
        public async Task GetNotification(int UserId)
        {
            bool isMealSend=false;
            bool isDiarySend=false;
            var user = _context.Users.Single(x => x.Id == UserId);
            var userInfo = _context.Userinfos.Single(x => x.UserId == UserId);
            var lastMealTime = _context.Meals.Where(x => x.UserId == UserId).OrderByDescending(x => x.MealTime).FirstOrDefault()?.MealTime;
            if (lastMealTime != null && lastMealTime < DateTime.UtcNow.AddDays(-1))
                isMealSend = true;
            if(userInfo.LastlessonTime<DateTime.UtcNow.AddDays(-1))
                isDiarySend = true;
            //if (isMealSend && isDiarySend)
            //    await SendNotification(user.UserNoId, _bothmess);

        }
    }

    public class NocodeNot
    {
        public string message_id { get; set; }

        public long client_id { get; set; }
    }
}
