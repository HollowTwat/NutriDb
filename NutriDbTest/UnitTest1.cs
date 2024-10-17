using Newtonsoft.Json;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System.Text;

namespace NutriDbTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //var _telegramBotClient = ErrorHelper.GetTelegramBot();
            //ErrorHelper.SendSystemMess("À¿À¿À");
            var a = new GPTAnswerResponse() { IsError = true, Answer=Newtonsoft.Json.JsonConvert.SerializeObject(new GPTResponse() { extra="extra", food= new List<NutriDbService.PythModels.PythFood> { }, pretty="pretty" }) }; 
           var b=JsonConvert.SerializeObject(a);
            decimal[] values = new decimal[] { 2222.5m, 3132.0m, 2345.7m, 2912.1m, 3123.3m, 1123.1m, 1954.6m };
            string[] labels = new string[] { "Cat 1", "Cat 2", "Cat 3", "Cat 4", "Cat 5", "Cat 6", "Cat 7" };
            long tgId = 389054202;
            PlotHelper plotHelper = new PlotHelper();
            plotHelper.SendPlot(values, labels, tgId, 2595.00M);
            Assert.True(true);
        }
        [Fact]
        public async void Test2()
        {
            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(800);
                EditMealRequest request = new EditMealRequest()
                {
                    userTgId = 186556585,
                    EatedAt = DateTime.Now.AddDays(-i).ToString("dd.MM.yyyy_HH:mm"),
                    meal = new NutriDbService.PythModels.PythMeal
                    {
                        description = "TestBreak",
                        totalWeight = 350,
                        type = NutriDbService.PythModels.mealtype.breakfast,
                        food = new List<NutriDbService.PythModels.PythFood>
                         {
                            new NutriDbService.PythModels.PythFood
                            {
                                 description="Foodp1",
                                weight=200,
                                nutritional_value= new NutriDbService.PythModels.NutriProps
                                 {
                                     fats=30, carbs= 60, protein= 5, kcal=530
                                 }
                            },
                             new NutriDbService.PythModels.PythFood
                            {
                                 description="Foodp2",
                                weight=150,
                                nutritional_value= new NutriDbService.PythModels.NutriProps
                                 {
                                     fats=1, carbs= 1, protein= 15, kcal=73
                                 }
                            }
                         }
                    }
                };
                var reqUrl = "https://nutridb-production.up.railway.app/api/TypesCRUD/CreateMeal";
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(90);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(reqUrl, content);
                var r = await response.Content.ReadAsStringAsync();
            }
            Assert.True(true);
        }
    }
}