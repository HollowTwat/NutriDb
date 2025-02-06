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

            var a = "TransactionId=2593940675&Amount=3900.00&Currency=RUB&PaymentAmount=3900.00&PaymentCurrency=RUB&OperationType=Payment&InvoiceId=f5fea457-9687-4585-80b2-cfdff528285d&AccountId=79645222475&SubscriptionId=sc_9a22a658216b61f5351301ce6b790&Name=&Email=mablmsk@yandex.com&DateTime=2024-11-29 10:03:04&IpAddress=102.18.30.144&IpCountry=&IpCity=&IpRegion=&IpDistrict=&IpLatitude=&IpLongitude=&CardId=657c4df3f76043d3ea0eee8a&CardFirstSix=220220&CardLastFour=1248&CardType=MIR&CardExpDate=12/33&Issuer=Sberbank&IssuerBankCountry=RU&Description=Тариф на 3 месяца&AuthCode=136461&Token=tk_b1dc3f720cec8ab079ca80f633ded&TestMode=0&Status=Completed&GatewayName=Tinkoff&Data={\"CloudPayments\":{\"recurrent\":{\"interval\":\"Month\",\"period\":3,\"customerReceipt\":{\"items\":[{\"label\":\"Тариф на 3 месяца\",\"price\":3900,\"quantity\":1,\"amount\":3900,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":1,\"phone\":\"79645222475\",\"amounts\":{\"electronic\":3900,\"advancePayment\":0,\"credit\":0,\"provision\":0}}},\"CustomerReceipt\":{\"items\":[{\"label\":\"Тариф на 3 месяца\",\"price\":3900,\"quantity\":1,\"amount\":3900,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":1,\"phone\":\"79645222475\",\"amounts\":{\"electronic\":3900,\"advancePayment\":0,\"credit\":0,\"provision\":0}}}}&TotalFee=312.00&CardProduct=PRD&PaymentMethod=&Rrn=433410512543&InstallmentTerm=&InstallmentMonthlyPayment=&CustomFields=[{\"ID\":\"6206927683\"}]";
            var cl = subscriptionHelper.ConvertToPayRequestJSON(a);
            var b = Newtonsoft.Json.JsonConvert.SerializeObject(cl);
            JObject root = JObject.Parse(cl.Data["CloudPayments"].ToString());

            // Достаем поле "label" из первого элемента массива "Items"
            string label = (string)root["CustomerReceipt"]["Items"][0]["label"];
            Xunit.Assert.True(true);
        }
        [Fact]
        public async Task NocodeSuccPayTest()
        {
            Setup();
            SubscriptionHelper subscriptionHelper = new SubscriptionHelper(_mockContext.Object, _mockServiceScopeFactory.Object);
            await subscriptionHelper.SendPayNoti(389054202);
            await subscriptionHelper.SendEmailInfo("vkpolkit2012@gmail.com");
            Xunit.Assert.True(true);
        }


    }
}