using NutriDbService.Helpers;

namespace NutriDbTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            decimal[] values = new decimal[] { 2222.5m, 3132.0m, 2345.7m, 2912.1m, 3123.3m, 1123.1m, 1954.6m };
            string[] labels = new string[] { "Cat 1", "Cat 2", "Cat 3", "Cat 4", "Cat 5", "Cat 6", "Cat 7" };
            long tgId = 389054202;
            PlotHelper plotHelper = new PlotHelper();
            plotHelper.SendPlot(values, labels, tgId, 2595.00M);
            Assert.True(true);
        }
    }
}