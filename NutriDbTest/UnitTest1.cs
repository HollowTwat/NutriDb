using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NutriDbService.DbModels;
using NutriDbService.Helpers;

namespace NutriDbTest
{
    [TestFixture]
    public class UnitTest1
    {
        private Mock<railwayContext> _mockContext;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<IServiceScope> _mockScope;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ILogger<NotificationHelper>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<railwayContext>();
            _mockLogger = new Mock<ILogger<NotificationHelper>>();
            // Создайте мок для ServiceProvider
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<NotificationHelper>)))
                                .Returns(_mockLogger.Object);
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
            await notificationHelper.SendNotification(17);
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

    }
}