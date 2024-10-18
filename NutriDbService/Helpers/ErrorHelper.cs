using System;
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
        public static void SendSystemMess(string mess)
        {
            var message = $"SYSTEM MESS:\n{mess}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                    GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart).GetAwaiter().GetResult();
                }
            }
            else
                GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static void SendErrorMess(string mess, Exception ex)
        {
            var message = $"ERROR MESS:{DateTime.UtcNow.ToLocalTime().AddHours(3)} \n {mess} \n\n Exception Text:\n{Newtonsoft.Json.JsonConvert.SerializeObject(ex)}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                    GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart).GetAwaiter().GetResult();
                }
            }
            else
                GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static void SendErrorMess(string mess)
        {
            var message = $"ERROR MESS:{DateTime.UtcNow.ToLocalTime().AddHours(3)} \n Error Text:\n{mess}";
            if (message.Length > 4000)
            {
                for (int i = 0; i < message.Length / 4000 + 1; i++)
                {
                    var messagePart = message.Substring(i * 4000, message.Length - i * 4000 > 4000 ? 4000 : message.Length - i * 4000);
                    GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), messagePart).GetAwaiter().GetResult();
                }
            }
            else
                GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), message).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
    }
}
