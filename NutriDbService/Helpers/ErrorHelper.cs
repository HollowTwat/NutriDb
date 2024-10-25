using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NutriDbService.Helpers
{
    public static class ErrorHelper
    {

        private const long _errorChanelId = -1002345895875;
        private static string _token = "6719978038:AAFFTz8Tat9ieYUyCp-tI2RAbcpddqTcZNY";
        private static TelegramBotClient client { get; set; }
        public static TelegramBotClient GetTelegramBot()
        {
            if (client != null)
            {
                return client;
            }
            client = new TelegramBotClient(_token);
            return client;
        }
        public static async Task SendSystemMess(string mess)
        {
            var message = $"SYSTEM MESS:\n{mess}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                 await   GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart);
                }
            }
            else
                await GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message);
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static async Task SendErrorMess(string mess, Exception ex)
        {
            var message = $"ERROR MESS:{TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Russian Standard Time")} \n {mess} \n\n Exception Text:\n{Newtonsoft.Json.JsonConvert.SerializeObject(ex)}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                    await GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart);
                }
            }
            else
                await GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message);
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static async Task SendErrorMess(string mess)
        {
            var message = $"ERROR MESS:{TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Russian Standard Time")} \n Error Text:\n{mess}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                    await GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart);
                }
            }
            else
                await GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message);
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
    }
}
