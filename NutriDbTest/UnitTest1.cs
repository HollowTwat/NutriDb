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
            await notificationHelper.SendNotification(3,true);
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

            var a = "TransactionId=2576174960&Amount=10.00&Currency=RUB&PaymentAmount=10.00&PaymentCurrency=RUB&OperationType=Payment&InvoiceId=39915412-3472-4f46-ba3e-6b9e3fa142c6&AccountId=79067444962&SubscriptionId=sc_bf31902f8e950c0b3a32394299b9b&Name=&Email=&DateTime=2024-11-15 21:53:25&IpAddress=213.87.86.211&IpCountry=RU&IpCity=Москва&IpRegion=Москва&IpDistrict=Москва&IpLatitude=55.75222&IpLongitude=37.61556&CardId=5f5cdcf10c67c43150f37d9e&CardFirstSix=437772&CardLastFour=3451&CardType=Visa&CardExpDate=02/25&Issuer=T-Bank (Tinkoff)&IssuerBankCountry=RU&Description=тестовая подписка2&AuthCode=A1B2C3&Token=tk_ca8cdb82e6ff9298e1dc04b44313a&TestMode=1&Status=Completed&GatewayName=Test&Data={\"CloudPayments\":{\"recurrent\":{\"interval\":\"Month\",\"period\":1,\"customerReceipt\":{\"items\":[{\"label\":\"тестовая подписка2\",\"price\":10,\"quantity\":1,\"amount\":10,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":0,\"phone\":\"79067444962\",\"amounts\":{\"electronic\":10,\"advancePayment\":0,\"credit\":0,\"provision\":0}}},\"CustomerReceipt\":{\"items\":[{\"label\":\"тестовая подписка2\",\"price\":10,\"quantity\":1,\"amount\":10,\"object\":4,\"method\":4,\"unitCode\":0}],\"isBso\":false,\"taxationSystem\":0,\"phone\":\"79067444962\",\"amounts\":{\"electronic\":10,\"advancePayment\":0,\"credit\":0,\"provision\":0}}}}&TotalFee=0.00&CardProduct=N1&PaymentMethod=&InstallmentTerm=&InstallmentMonthlyPayment=&CustomFields=[{\"id\":\"3\"}]";
           var cl= subscriptionHelper.ConvertToPayRequestJSON(a);

            Xunit.Assert.True(true);
        }
        [Fact]
        public async Task NocodeSuccPayTest()
        {
            Setup();
            SubscriptionHelper subscriptionHelper = new SubscriptionHelper(_mockContext.Object, _mockServiceScopeFactory.Object);
            await subscriptionHelper.SendPayNoti(389054202);
            Xunit.Assert.True(true);
        }
    }
}