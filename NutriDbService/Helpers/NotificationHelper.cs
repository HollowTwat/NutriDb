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
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

namespace NutriDbService.Helpers
{
    public class NotificationHelper
    {

        //private readonly static string _api_key = "f3ccf95cf601c3fb7efe18c3b6135d4a";
        //private readonly static string _lessondonemess = "36169317";
        //private readonly static string _lessonforgotmess = "36327038";
        //private readonly static string _ndiarynmealmess = "36645266";
        private string Htoken = "7220622235:AAEJAQUjZZagg6ZXkGuykfQySAtJzwAwqRI";
        private railwayContext _context;
        private readonly ILogger _logger;

        public NotificationHelper(railwayContext context, IServiceScopeFactory serviceProviderFactory)
        {
            _context = context;
            _logger = serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<NotificationHelper>>();
        }
        //private async Task SendNot(long? ClientId, string MessBoxId)
        //{
        //    if (ClientId == null) { throw new ArgumentNullException("ClientId"); }
        //    var reqparams = new NocodeNot { client_id = (long)ClientId, message_id = MessBoxId };
        //    HttpClient client = new HttpClient();
        //    client.Timeout = TimeSpan.FromSeconds(100);
        //    HttpContent content = new StringContent(JsonConvert.SerializeObject(reqparams), Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync(_reqUrl, content);
        //    var r = await response.Content.ReadAsStringAsync();
        //}
        private async Task SendNotH(long? ClientId, bool isMorning, string lessonNo)
        {
            var botClient = new TelegramBotClient(Htoken);
            var text = isMorning ? "Начать урок" : "Продолжить урок";
            var lessonb = InlineKeyboardButton.WithCallbackData(text, $"d{lessonNo}");
            var downb = InlineKeyboardButton.WithCallbackData("⏏️", "menu_back");
            var buttons = new List<List<InlineKeyboardButton>> { new List<InlineKeyboardButton> { lessonb }, new List<InlineKeyboardButton> { downb } };
            // Отправка изображения
            await botClient.SendTextMessageAsync(
                chatId: ClientId,
                text: isMorning ? "Утреннее уведомление" : "Вечернее уведомление",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );
        }
        //public async Task SendNotification(int UserId, bool isMornong)
        //{
        //    try
        //    {
        //        _logger.LogWarning($"User:{UserId} SendNotification");
        //        bool isMealNotSend = false;
        //        bool isLessonForgotSend = false;
        //        bool isLessonDoneSend = false;
        //        var user = await _context.Users.SingleAsync(x => x.Id == UserId);
        //        var userInfo = await _context.Userinfos.SingleAsync(x => x.UserId == UserId);
        //        if (!userInfo.Donelessonlist.Contains("21"))
        //        {
        //            if (userInfo.LastlessonTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
        //                isLessonForgotSend = true;
        //            else
        //                isLessonDoneSend = true;
        //        }
        //        if (isMornong)
        //        {
        //            if (isLessonForgotSend)
        //                await SendNot(user.UserNoId, _lessonforgotmess);
        //            if (isLessonDoneSend)
        //                await SendNot(user.UserNoId, _lessondonemess);
        //        }
        //        else
        //        {
        //            var meals = await _context.Meals.Where(x => x.UserId == UserId).OrderByDescending(x => x.MealTime).FirstOrDefaultAsync();

        //            var lastMealTime = meals?.MealTime;
        //            if (lastMealTime != null && lastMealTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
        //                isMealNotSend = true;
        //            if (isMealNotSend && isLessonForgotSend)
        //                await SendNot(user.UserNoId, _ndiarynmealmess);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"NotificationSendError for User:{UserId}", ex);
        //        await ErrorHelper.SendErrorMess($"NotificationSendError for User:{UserId}", ex);
        //    }

        //}
        public async Task SendNotificationH(UserPing userPing, bool isMornong)
        {
            try
            {
                List<string> doubleLessPrev = new List<string>() { "1", "2", "3", "4", "5", "6", "8", "9", "10", "11", "12", "14", "15", "16" };
                _logger.LogWarning($"User:{userPing.UserTgId} SendNotification");
                //bool isMealNotSend = false;
                bool isLessonForgotSend = false;
                //var user = await _context.Users.SingleAsync(x => x.Id == UserId);
                var userInfo = await _context.Userinfos.SingleAsync(x => x.UserId == userPing.UserId);
                var lesList = userInfo.Donelessonlist.Split(',');

                if (!userInfo.Donelessonlist.Contains("21"))
                {
                    if (userInfo.LastlessonTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
                        isLessonForgotSend = true;
                }
                if (isMornong)
                {
                    if (isLessonForgotSend)
                        await SendNotH(userPing.UserTgId, true, lesList.Last());
                    //if (isLessonDoneSend)
                    //    await SendNotH(user.UserNoId,true, lesList.Last());
                }
                else
                {
                    //var meals = await _context.Meals.Where(x => x.UserId == UserId).OrderByDescending(x => x.MealTime).FirstOrDefaultAsync();

                    //var lastMealTime = meals?.MealTime;
                    //if (lastMealTime != null && lastMealTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1))
                    //    isMealNotSend = true;
                    //if (isMealNotSend && isLessonForgotSend)
                    if (doubleLessPrev.Contains(lesList.Last()))
                        await SendNotH(userPing.UserTgId, false, lesList.Last());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotificationSendError for User:{userPing.UserTgId}", ex);
                await ErrorHelper.SendErrorMess($"NotificationSendError for User:{userPing.UserTgId}", ex);
            }

        }
    }

    public class NocodeNot
    {
        public string message_id { get; set; }

        public long client_id { get; set; }
    }
}
