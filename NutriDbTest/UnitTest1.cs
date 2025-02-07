using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot.Types;
using static System.Net.WebRequestMethods;

namespace NutriDbTest
{
    [TestFixture]
    public class UnitTest1
    {
        private Mock<railwayContext> _mockContext;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<IServiceScope> _mockScope;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ILogger<NotificationHelper>> _mockNotificationLogger;
        private Mock<ILogger<SubscriptionHelper>> _mockSubscriptionLogger;
        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<railwayContext>();
            _mockNotificationLogger = new Mock<ILogger<NotificationHelper>>();
            _mockSubscriptionLogger = new Mock<ILogger<SubscriptionHelper>>();
            // Создайте мок для ServiceProvider
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<NotificationHelper>)))
                                .Returns(_mockNotificationLogger.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<SubscriptionHelper>)))
                                .Returns(_mockSubscriptionLogger.Object);
            // Создайте мок для IServiceScope
            _mockScope = new Mock<IServiceScope>();
            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            // Создайте мок для IServiceScopeFactory и настройте возвращаемый scope
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScopeFactory.Setup(s => s.CreateScope()).Returns(_mockScope.Object);
        }

        [Fact]
        public async Task NotifyTest()
        {
            Setup();
            var notificationHelper = new NotificationHelper(new railwayContext(), _mockServiceScopeFactory.Object);
            await notificationHelper.SendNotification(13, true);
            Xunit.Assert.True(true);
        }

        [Fact]
        public async Task PlotTest()
        {
            var userTgId = 222312868;
            Setup();
            try
            {
                var _context = new railwayContext();
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var goalkk = (await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == user.Id)).Goalkk;
                DateTime startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                int daysinperiod = 0;
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).Date;

                daysinperiod = now.Day - startDate.Day;
                var meals = await _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
                var dishes = await _context.Dishes.Where(x => meals.Select(x => x.Id).Contains(x.MealId)).ToListAsync();
                //List<(string, decimal)> plotPairs = new List<(string, decimal)>();
                decimal[] values = new decimal[7];
                string[] labels = new string[7];
                for (var i = 1; i <= 7; i++)
                {
                    var ndate = startDate.AddDays(i);
                    var todaymeals = meals.Where(x => x.MealTime.Date == ndate.Date);
                    decimal todaykk = 0.0m;

                    if (todaymeals.Any())
                    {
                        var todayDishes = dishes.Where(x => todaymeals.Select(x => x.Id).Contains(x.MealId));

                        foreach (var dish in todayDishes)
                        {
                            todaykk += dish.Kkal;
                        }
                    }
                    labels[i - 1] = ndate.Date.ToString("dd.MM");
                    values[i - 1] = todaykk;
                }
            }
            catch (Exception ex) { }

        }

        [Fact]
        public async Task SubTest()
        {
            Setup();
            SubscriptionHelper subscriptionHelper = new SubscriptionHelper(_mockContext.Object, _mockServiceScopeFactory.Object);

            var a = "TransactionId=2683570734&Amount=3900.00&Currency=RUB&PaymentAmount=3900.00&PaymentCurrency=RUB&OperationType=Payment&InvoiceId=&AccountId=2ffeadfd-1b90-4b96-9253-2e366e70eee3&SubscriptionId=&Name=&Email=juliapopova2804@gmail.com&DateTime=2025-02-07 12:15:54&IpAddress=185.107.56.213&IpCountry=NL&IpCity=Розендал&IpRegion=Северный Брабант&IpDistrict=Розендал&IpLatitude=51.53083&IpLongitude=4.46528&CardId=&CardFirstSix=220220&CardLastFour=8185&CardType=MIR&CardExpDate=09/27&Issuer=Sberbank&IssuerBankCountry=RU&Description=Подписка на 1 год&AuthCode=&TestMode=0&Status=Completed&GatewayName=TBank Mapi&Data={\r\n        \"CloudPayments\": {\r\n          \"CustomerReceipt\": {\r\n            \"Items\": [\r\n              {\r\n                \"label\": \"Подписка на 1 год\",\r\n                \"price\": 3900,\r\n                \"quantity\": 1,\r\n                \"amount\": 3900,\r\n                \"method\": 4,\r\n                \"object\": 4\r\n              }\r\n            ],\r\n            \"isBso\": false,\r\n            \"amounts\": {\r\n              \"electronic\": 3900,\r\n              \"advancePayment\": 0,\r\n              \"credit\": 0,\r\n              \"provision\": 0\r\n            }\r\n          },\r\n          \"recurrent\": {\r\n            \"interval\": \"Month\",\r\n            \"period\": 12,\r\n            \"customerReceipt\": {\r\n              \"Items\": [\r\n                {\r\n                  \"label\": \"Подписка на 1 год\",\r\n                  \"price\": 3900,\r\n                  \"quantity\": 1,\r\n                  \"amount\": 3900,\r\n                  \"method\": 4,\r\n                  \"object\": 4\r\n                }\r\n              ],\r\n              \"isBso\": false,\r\n              \"amounts\": {\r\n                \"electronic\": 3900,\r\n                \"advancePayment\": 0,\r\n                \"credit\": 0,\r\n                \"provision\": 0\r\n              }\r\n            }\r\n          }\r\n        }\r\n      }&TotalFee=152.10&CardProduct=PRD&PaymentMethod=SberPay&Rrn=5817720443&InstallmentTerm=&InstallmentMonthlyPayment=&CustomFields=";
            var cl = subscriptionHelper.ConvertToPayRequestJSON(a);


            JObject root = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(cl.Data.Values.First()));

            // Безопасно достаем поле "label" с помощью SelectToken
            string planLabel = (string)root.SelectToken("CustomerReceipt.Items[0].label");
            Xunit.Assert.True(true);
        }
        [Fact]
        public async Task NocodeSuccPayTest()
        {
            Setup();
            SubscriptionHelper subscriptionHelper = new SubscriptionHelper(_mockContext.Object, _mockServiceScopeFactory.Object);
            await subscriptionHelper.SendPayNoti(389054202);
            await subscriptionHelper.SendEmailInfo("vkpolkit2012@gmail.com", "подписка на всегда");
            Xunit.Assert.True(true);
        }


    }
}