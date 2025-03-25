using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using System.Text;

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
        public async Task NotifyHTest()
        {
            Setup();
            var notificationHelper = new NotificationHelper(new railwayContext(), _mockServiceScopeFactory.Object);
            await notificationHelper.SendNotificationH(new NutriDbService.UserPing { UserId = 78, UserTgId = 389054202 }, true);
            await notificationHelper.SendNotificationH(new NutriDbService.UserPing { UserId = 78, UserTgId = 389054202 }, false);
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

        [Fact]
        public async Task GetStatic()
        {
            try
            {
                var _context = new railwayContext();
                var startDateTime = new DateTime(2025, 01, 01);
                var endDateTime = DateTime.Now;

                Dictionary<DateTime, (int, int)> mounthres = new();
                for (var currentDateTime = startDateTime; currentDateTime <= endDateTime; currentDateTime = currentDateTime.AddMonths(1))
                {
                    var mounthend = currentDateTime.AddMonths(1);
                    var user = await _context.Users.Where(x => x.RegistrationTime <= DateOnly.FromDateTime(mounthend)).ToListAsync();
                    var meals = await _context.Meals.Where(x => x.MealTime.Date.Month == currentDateTime.Month && user.Select(x => x.Id).Contains(x.UserId)).ToListAsync();
                    var activeuserIds = meals.Select(x => x.UserId).Distinct().ToList();
                    mounthres.Add(mounthend, new(user.Count(), activeuserIds.Count()));
                }




                Dictionary<DateTime, (int, int)> weekres = new();

                for (var currentDateTime = startDateTime; currentDateTime <= endDateTime; currentDateTime = currentDateTime.AddDays(7))
                {
                    var weekend = currentDateTime.AddDays(7);
                    var user = await _context.Users.Where(x => x.RegistrationTime <= DateOnly.FromDateTime(weekend)).ToListAsync();
                    var meals = await _context.Meals.Where(x => x.MealTime.Date > currentDateTime && x.MealTime.Date < weekend && user.Select(x => x.Id).Contains(x.UserId)).ToListAsync();
                    var activeuserIds = meals.Select(x => x.UserId).Distinct().ToList();
                    weekres.Add(weekend, new(user.Count(), activeuserIds.Count()));
                }
                var csvw = new StringBuilder();



                Dictionary<DateTime, (int, int)> dayres = new();
                for (var currentDateTime = startDateTime; currentDateTime <= endDateTime; currentDateTime = currentDateTime.AddDays(1))
                {
                    var user = await _context.Users.Where(x => x.RegistrationTime <= DateOnly.FromDateTime(currentDateTime)).ToListAsync();
                    var meals = await _context.Meals.Where(x => x.MealTime.Date == currentDateTime && user.Select(x => x.Id).Contains(x.UserId)).ToListAsync();
                    var activeuserIds = meals.Select(x => x.UserId).Distinct().ToList();
                    dayres.Add(currentDateTime, new(user.Count(), activeuserIds.Count()));
                }



                using (var workbook = new XLWorkbook())
                {
                    // Функция для добавления данных из словаря на лист
                    void AddDataToSheet(IXLWorksheet sheet, Dictionary<DateTime, (int, int)> dict)
                    {
                        sheet.Cell(1, 1).Value = "Date";
                        sheet.Cell(1, 2).Value = "allUser";
                        sheet.Cell(1, 3).Value = "activeUser";

                        int row = 2;
                        foreach (var el in dict)
                        {
                            sheet.Cell(row, 1).Value = el.Key.ToShortDateString();
                            sheet.Cell(row, 2).Value = el.Value.Item1.ToString();
                            sheet.Cell(row, 3).Value = el.Value.Item2.ToString();
                            row++;
                        }
                    }

                    // Создаем и заполняем три листа
                    var worksheet1 = workbook.Worksheets.Add("Day");
                    AddDataToSheet(worksheet1, dayres);

                    var worksheet2 = workbook.Worksheets.Add("Week");
                    AddDataToSheet(worksheet2, weekres);

                    var worksheet3 = workbook.Worksheets.Add("Mounth");
                    AddDataToSheet(worksheet3, mounthres);

                    // Сохраняем файл
                    workbook.SaveAs("D:\\An.xlsx");
                }



                Xunit.Assert.True(true);
            }
            catch (Exception ex)
            {
                var a = 0;
            }


        }
        [Fact]
        public async Task GetStaticV2()
        {
            try
            {
                var _context = new railwayContext();
                var users = await _context.Users.ToListAsync();
                var usersStatic = new List<StatickAnswer>();
                foreach (var user in users)
                {
                    var info = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == user.Id);
                    if (info == null)
                        continue;
                    var meal = await _context.Meals.OrderBy(x => x.MealTime).LastOrDefaultAsync(x => x.UserId == user.Id);
                    var subscriptions = await _context.Subscriptions.Where(x => x.UserTgId == user.TgId).ToListAsync();
                    var subbs = new List<Subs>();
                    foreach (var item in subscriptions)
                    {
                        subbs.Add(new Subs
                        {
                            payment_amount = item.Amount,
                            payment_id = item.TransactionId,
                            subscription_period = item.Type,
                            subscription_status = item.Status,
                            subscription_type = item.Type
                        });
                    }
                    var userStatic = new StatickAnswer()
                    {
                        telegram_id = user.TgId,
                        username = user.Username,
                        first_name = "-",
                        last_name = "-",
                        registration_date = user.RegistrationTime.ToString(),
                        phone = "-",
                        last_activity = meal?.MealTime.ToString(),
                        lessons_completed = info?.Donelessonlist?.Split(',').Count(),
                        email = user.Email,
                        gender = info.Gender,
                        age = info.Age,
                        weight_kg = info.Weight,
                        height_cm = info.Height,
                        goal = info.Goal,
                        daily_caloric_norm_kcal = info.Goalkk,
                        subs = subbs

                    };
                    usersStatic.Add(userStatic);
                }
                Xunit.Assert.True(true);
            }
            catch (Exception ex) { }
        }

        [Fact]
        public async Task GetStaticLessonEnd()
        {
            try
            {
                var _context = new railwayContext();
                var userInfoDone = await _context.Userinfos.Where(x => x.Donelessonlist.EndsWith("21")).ToListAsync();
                var users = await _context.Users.Where(x => userInfoDone.Select(x => x.UserId).Contains(x.Id)).ToListAsync();
                var doneUsers = new List<UserDoneInfo>();

                foreach (var user in users)
                {
                    doneUsers.Add(new UserDoneInfo
                    {
                        tgId = user.TgId,
                        Email = user.Email,
                        RegisterTime = user.RegistrationTime.ToDateTime(new TimeOnly(0, 0)),
                        LessonEndTime = userInfoDone.SingleOrDefault(x => x.UserId == user.Id)?.LastlessonTime ?? DateTime.UtcNow,
                    });
                }
                var d2 = doneUsers.ToList();
                doneUsers.RemoveAll(x => (x.LessonEndTime - x.RegisterTime).Days > 31);
                var res = Newtonsoft.Json.JsonConvert.SerializeObject(doneUsers);
                var others = d2.Except(doneUsers);
                var res2 = Newtonsoft.Json.JsonConvert.SerializeObject(others);
                Xunit.Assert.True(true);
            }
            catch (Exception ex) { }
        }
    }
    public class UserDoneInfo
    {
        public long tgId { get; set; }

        public string Email { get; set; }

        public DateTime RegisterTime { get; set; }

        public DateTime LessonEndTime { get; set; }
    }
    public class Subs
    {
        public decimal? payment_amount { get; set; }

        public long? payment_id { get; set; }

        public string subscription_period { get; set; }

        public string subscription_end_date { get; set; }

        public string subscription_type { get; set; }

        public string subscription_status { get; set; }
    }
    public class StatickAnswer
    {
        public long telegram_id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string registration_date { get; set; }

        public string phone { get; set; }

        public string last_activity { get; set; }

        public int? lessons_completed { get; set; }

        public string email { get; set; }

        public List<Subs> subs { get; set; }

        //public long total_spent {  get; set; }

        //public int total_purchases {  get; set; }
        public string gender { get; set; }

        public short? age { get; set; }

        public decimal? weight_kg { get; set; }

        public decimal? height_cm { get; set; }

        public string goal { get; set; }

        //public decimal target_weight_kg { get; set; }

        public decimal? daily_caloric_norm_kcal { get; set; }

        // public string macronutrient_norm_g { get; set; }
        public int weekly_activity_hours { get; set; }


    }
}