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
using Telegram.Bot.Types;

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
        private readonly Dictionary<int, (string, string)> messText;
        private MealHelper _mealHelper;
        public NotificationHelper(railwayContext context, MealHelper mealHelper, IServiceScopeFactory serviceProviderFactory)
        {
            _context = context;
            _logger = serviceProviderFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<NotificationHelper>>();
            _mealHelper = mealHelper;
            messText = new Dictionary<int, (string, string)> {
            {1,("Занести еду — 3 минуты. А чувство гордости за дисциплину — целый день!","Занести еду") },
            {2,("Нутри волнуется: вы съели что-то вкусное и не поделились?","Поделиться едой") },
            {3,("Засчитано только то, что записано. Сорри, такие правила в дневнике питания 📝","Отправить еду") },
            {4,("Не дай приёму пищи пропасть без вести. Ему положено быть в дневник!","Сделать запись") },
            {5,("Ваша тарелка скучает по вниманию. Пора занести её в Нутри!","Заполнить дневник") },
            {6,("🍽 Эй, не записал тарелку — как будто и не ел! Ну, почти.","Занести еду") },
            {7,("👀Кто съел обед и не поделился с дневником? Ну-ну.","Отправить еду") },
            {8,("🍎Даже яблоко хочет быть отмеченным. Ну правда!","Внести перекус") },
            {9,("Эй, ваш завтрак просится в дневник!","Отметить еду.") },
            {10,("Ужин съеден? Пора и КБЖУ посчитать","Занести в дневник") },
            {11,("А ну признавайтесь, что ели! Нутри любит честность 😇"," Занести еду") },
            {12,("👀 Нутри шепчет: «Покажи мне свою тарелку…»","Отправить фото еды") },
            {13,("🔮 У Нутри интуиция: вы что-то вкусное съели и не записали!","Поправить ситуацию") },
            {14,("Дневник жаждет внимания. Накормите его своей тарелкой 😄","Записать приём") },
            {15,("Котики мурлыкают, когда их гладят. А Нутри — когда вы вносите еду 💚","Порадовать Нутри") },
            {16,("💪 3 минуты — и вы снова герой здорового питания!","Внести еду") },
            {17,("Поели и ушли? А сделать запись в дневнике? 💭","Записать тарелку") },
            {18,("📖 А вы сегодня уже радовали свой дневник?","Занести приём пищи") },
            {19,("Один клик по кнопке — и вы снова ответственный человек!","Заполнить дневник") },
            {20,("Напоминаем: у Нутри слабость к красиво записанным блюдам ✨","Внести запись") },
            };
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
        private async Task SendNotH(long? ClientId, bool isMorning, int lessonNo)
        {
            var botClient = new TelegramBotClient(Htoken);
            if (lessonNo == 99)
                lessonNo = 0;
            if (isMorning)
                lessonNo++;
            var text = isMorning ? $"Начать урок {lessonNo}" : $"Продолжить урок {lessonNo}";


            var callback = isMorning ? $"d{lessonNo}" : $"d{lessonNo}_2";
            var lessonb = InlineKeyboardButton.WithCallbackData(text, callback);
            var downb = InlineKeyboardButton.WithCallbackData("⏏️", "menu_back");
            var buttons = new List<List<InlineKeyboardButton>> { new List<InlineKeyboardButton> { lessonb }, new List<InlineKeyboardButton> { downb } };
            // Отправка изображения
            await botClient.SendTextMessageAsync(
                chatId: ClientId,
                text: isMorning ? "Утреннее уведомление" : "Вечернее уведомление",
                replyMarkup: new InlineKeyboardMarkup(buttons)
            ).ConfigureAwait(false);
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
                List<int> doubleLessPrev = new List<int>() { 2, 3, 4, 5, 6, 8, 9, 10, 11, 12, 13, 15, 16, 17 };
                _logger.LogWarning($"User:{userPing.UserTgId} SendNotification");
                //bool isMealNotSend = false;
                bool isLessonForgotSend = false;
                //var user = await _context.Users.SingleAsync(x => x.Id == UserId);
                var userInfo = await _context.Userinfos.AsNoTracking().SingleAsync(x => x.UserId == userPing.UserId).ConfigureAwait(false);

                if (userInfo == null)
                {
                    _logger.LogError("User info not found for user {UserId}", userPing.UserId);
                    return;
                }
                if (int.TryParse(userInfo.Donelessonlist.Split(',').Last(), out int lasLes))
                {

                    if (lasLes != 21)
                    {
                        if (userInfo.LastlessonTime < DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(userInfo?.Timeslide ?? 0)).AddHours(-12))
                            isLessonForgotSend = true;
                    }
                    if (isMornong)
                    {
                        if (isLessonForgotSend)
                            await SendNotH(userPing.UserTgId, true, lasLes).ConfigureAwait(false);
                        //if (isLessonDoneSend)
                        //    await SendNotH(user.UserNoId,true, lesList.Last());
                    }
                    else
                    {
                        if (doubleLessPrev.Contains(lasLes))
                            await SendNotH(userPing.UserTgId, false, lasLes);
                    }
                }
                else
                {
                    _logger.LogError($"NotificationSendError for User:{userPing.UserTgId}");
                    await ErrorHelper.SendErrorMess($"NotificationSendError for User:{userPing.UserTgId}");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotificationSendError for User:{userPing.UserTgId}", ex);
                await ErrorHelper.SendErrorMess($"NotificationSendError for User:{userPing.UserTgId}", ex);
            }

        }
        public async Task SendNotificationSingle(UserPing userPing)
        {
            try
            {
                _logger.LogWarning($"User:{userPing.UserTgId} SendNotification");

                var userInfo = await _context.Userinfos.AsNoTracking().SingleAsync(x => x.UserId == userPing.UserId).ConfigureAwait(false);

                if (userInfo == null)
                {
                    _logger.LogError("User info not found for user {UserId}", userPing.UserId);
                    return;
                }
                var resp = await _mealHelper.GetMealTotal(userPing.UserTgId, Periods.day);

                var botClient = new TelegramBotClient(Htoken);
                var mess = $"Ваша статистика на сегодня 🍽️\r\n\r\nДневная цель : {resp.GoalKkal} ккал., {resp.GoalProt} г. белки, {resp.GoalFats} г. жиры, {resp.GoalCarbs} г. углеводы 💪.\r\nСегодня вы съели {resp.TotalKkal} ккал.🔥\r\n\r\nБелки: {resp.TotalProt} г. 💪\r\nЖиры: {resp.TotalFats} г. \U0001f9c8\r\nУглеводы: {resp.TotalCarbs} г. 🍞\r\n\r\nВы можете еще съесть {resp.RemainingKK} ккал.";
                var dictVal = messText[new Random().Next(1, 21)];
                var callback = $"menu_dnevnik_input";
                var lessonb = InlineKeyboardButton.WithCallbackData(dictVal.Item2, callback);
                //var downb = InlineKeyboardButton.WithCallbackData("⏏️", "menu_back");
                var buttons = /*new List<List<InlineKeyboardButton>> {*/ new List<InlineKeyboardButton> { lessonb };/*, new List<InlineKeyboardButton> { downb } };*/
                // Отправка изображения
                await botClient.SendTextMessageAsync(
                    chatId: userPing.UserTgId,
                    text: dictVal.Item1,
                    replyMarkup: new InlineKeyboardMarkup(buttons)
                ).ConfigureAwait(false);

                await botClient.SendTextMessageAsync(
                   chatId: userPing.UserTgId,
                   text: mess
               ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotificationSendError for User:{userPing.UserTgId}", ex);
                await ErrorHelper.SendErrorMess($"NotificationSendError for User:{userPing.UserTgId}", ex);
            }

        }
        public async Task SendVoteNotificationSingle(long UserTgId)
        {
            try
            {
                _logger.LogWarning($"User:{UserTgId} SendNotification");

                var botClient = new TelegramBotClient(Htoken);
                var mess = $"🌿 <i> Как ты оценишь свое последнее взаимодействие с Нутри?</i>\r\n\r\nОцените от 1 до 10, где 1 —  «очень плохо», а 10 — «отлично».";
                var buttons = new List<InlineKeyboardButton>();
                for (int i = 1; i <= 10; i++)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData($"{i}", $"vote_{i}"));
                }
                List<InlineKeyboardButton> firstHalf = buttons.GetRange(0, 5);
                List<InlineKeyboardButton> secondHalf = buttons.GetRange(5, 5);
                await botClient.SendTextMessageAsync(
                    chatId: UserTgId,
                    text: mess,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>> { firstHalf, secondHalf })
                ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NotificationSendError for User:{UserTgId}", ex);
                await ErrorHelper.SendErrorMess($"NotificationSendError for User:{UserTgId}", ex);
            }

        }
        public async Task SendCustomMessToUserH(long TgId, string mess)
        {
            var botClient = new TelegramBotClient(Htoken);

            await botClient.SendTextMessageAsync(
                chatId: TgId,
                text: mess
            ).ConfigureAwait(false);
        }
    }

    public class NocodeNot
    {
        public string message_id { get; set; }

        public long client_id { get; set; }
    }
}
