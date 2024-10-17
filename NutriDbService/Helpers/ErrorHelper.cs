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
            mess = string.Concat("SYSTEM MESS\n", mess);
            GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), mess).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static void SendErrorMess(string mess, Exception ex)
        {
            mess = string.Concat($"ERROR MESS:\n {DateTime.UtcNow.ToLocalTime().AddHours(3)}", mess);

            mess = string.Concat($"{mess}\n Error Text:\n", Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), mess).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
        public static void SendErrorMess(string mess)
        {
            mess = string.Concat($"ERROR MESS\n {DateTime.UtcNow.ToLocalTime().AddHours(3)}", mess);

            mess = string.Concat($" Error Text:\n{mess}\n");
            GetTelegramBot().SendTextMessageAsync(new ChatId(_errorChanelId), mess).GetAwaiter().GetResult();
            //client.SendTextMessageAsync(new ChatId(464682207), mess).GetAwaiter().GetResult();
        }
    }
}
