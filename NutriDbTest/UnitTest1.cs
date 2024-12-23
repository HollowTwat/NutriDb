using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using Telegram.Bot.Types;

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
            var a = DateTime.TryParseExact("18.10.2024", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parseTime);
            decimal[] values = new decimal[] { 2222.5m, 3132.0m, 2345.7m, 2912.1m, 3123.3m, 1123.1m, 1954.6m };
            string[] labels = new string[] { "Cat 1", "Cat 2", "Cat 3", "Cat 4", "Cat 5", "Cat 6", "Cat 7" };
            long tgId = 389054202;
            PlotHelper plotHelper = new PlotHelper();
            await plotHelper.SendPlot(values, labels, tgId, 2595.00M);
            Xunit.Assert.True(true);
        }

        [Fact]
        public async Task SubTest()
        {
            Setup();
            SubscriptionHelper subscriptionHelper = new SubscriptionHelper(_mockContext.Object, _mockServiceScopeFactory.Object);

            var a = "TransactionId=2593940675&Amount=3900.00&Currency=RUB&PaymentAmount=3900.00&PaymentCurrency=RUB&OperationType=Payment&InvoiceId=f5fea457-9687-4585-80b2-cfdff528285d&AccountId=79645222475&SubscriptionId=sc_9a22a658216b61f5351301ce6b790&Name=&Email=mablmsk@yandex.com&DateTime=2024-11-29 10:03:04&IpAddress=102.18.30.144&IpCountry=&IpCity=&IpRegion=&IpDistrict=&IpLatitude=&IpLongitude=&CardId=657c4df3f76043d3ea0eee8a&CardFirstSix=220220&CardLastFour=1248&CardType=MIR&CardExpDate=12/33&Issuer=Sberbank&IssuerBankCountry=RU&Description=Тариф на 3 месяца&AuthCode=136461&Token=tk_b1dc3f720cec8ab079ca80f633ded&TestMode=0&Status=Completed&GatewayName=Tinkoff&Data={\"CloudPayments\":{\"recurrent\":{\"interval\":\"Month\",\"period\":3,\"customerReceipt\":{\"items\":[{\"label\":\"Тариф на 3 месяца\",\"price\":3900,\"quantity\":1,\"amount\":3900,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":1,\"phone\":\"79645222475\",\"amounts\":{\"electronic\":3900,\"advancePayment\":0,\"credit\":0,\"provision\":0}}},\"CustomerReceipt\":{\"items\":[{\"label\":\"Тариф на 3 месяца\",\"price\":3900,\"quantity\":1,\"amount\":3900,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":1,\"phone\":\"79645222475\",\"amounts\":{\"electronic\":3900,\"advancePayment\":0,\"credit\":0,\"provision\":0}}}}&TotalFee=312.00&CardProduct=PRD&PaymentMethod=&Rrn=433410512543&InstallmentTerm=&InstallmentMonthlyPayment=&CustomFields=[{\"ID\":\"6206927683\"}]";
            var cl = subscriptionHelper.ConvertToPayRequestJSON(a);

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