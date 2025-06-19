using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.IntegratorModels;
using NutriDbService.PayModel;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot.Types;

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TypesCRUDController : Controller
    {
        private readonly ILogger<TypesCRUDController> _logger;
        private railwayContext _context;
        private MealHelper _mealHelper;
        private PlotHelper _plotHelper;
        private readonly TaskSchedulerService _taskSchedulerService;
        private readonly NotificationHelper _notificationHelper;

        public TypesCRUDController(railwayContext context, MealHelper mealHelper, ILogger<TypesCRUDController> logger, PlotHelper plotHelper, NotificationHelper notificationHelper, TaskSchedulerService taskSchedulerService)
        {
            _context = context;
            _mealHelper = mealHelper;
            _logger = logger;
            _plotHelper = plotHelper;
            _notificationHelper = notificationHelper;
            _taskSchedulerService = taskSchedulerService;
        }

        #region AllCRUDS

        #region Meal
        [HttpPost]
        public async Task<int> CreateMeal(EditMealRequest request)
        {
            try
            {
                var mealId = await _mealHelper.CreateMeal(request);
                _logger.LogInformation($"UserTG={request.userTgId} Meal={mealId} was added");
                return mealId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal Create Error");
                await ErrorHelper.SendErrorMess("Meal Create Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {request}");
                return 0;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<bool> EditMeal(EditMealRequest request)
        {
            try
            {
                var res = await _mealHelper.EditMeal(request);
                _logger.LogInformation($"User={request.userTgId} Meal={res} was edited");
                return true;
                // return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal Edit Error");
                await ErrorHelper.SendErrorMess("Meal Edit Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {request}");
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<bool> DeleteMeal(int mealId, long userTgId)
        {
            try
            {
                var res = await _mealHelper.DeleteMeal(mealId, userTgId);
                if (res != 0)
                {
                    _logger.LogInformation($"User={userTgId} Meal={res} was deleted");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"User={userTgId} Meal={mealId} cant delete");
                    return false;
                }
                // return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal Edit Error");
                await ErrorHelper.SendErrorMess("Meal Delete Error", ex);
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        //[Obsolete]
        //[HttpGet]
        //public ActionResult<GetMealResponse> GetTodayUserMeals(long userTgId)
        //{
        //    try
        //    {
        //        var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
        //        if (user == null)
        //            throw new Exception($"I Cant Find User : {userTgId}");
        //        var meals = _context.Meals.Where(x => x.UserId == user.Id && DateTime.Equals(x.MealTime.Date, DateTime.UtcNow.ToLocalTime().AddHours(3).Date)).ToList();

        //        var mealsId = meals.Select(x => x.Id).ToList();
        //        var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
        //        var resp = new List<MealResponse>() { };
        //        foreach (var meal in meals)
        //        {
        //            resp.Add(new MealResponse()
        //            {
        //                mealId = meal.Id,
        //                eatedAt = meal.MealTime,
        //                userId = meal.UserId,
        //                meal = new PythModels.PythMeal
        //                {
        //                    description = meal.Description,
        //                    totalWeight = meal.Weight,
        //                    type = (mealtype)meal.Type,
        //                    food = dishes.Where(x => x.MealId == meal.Id).ToList().Select(x => new PythModels.PythFood()
        //                    {
        //                        description = x.Description,
        //                        weight = x.Weight,
        //                        nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein, x.Kkal)
        //                    }).ToList()
        //                }
        //            }
        //            );
        //        }
        //        return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponse(resp)));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
        //    }
        //}

        [HttpGet]
        public async Task<ActionResult<GetMealResponse>> GetUserMealById(long userTgId, long mealId)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var meal = await _context.Meals.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user.Id && x.Id == mealId);
                if (meal == null)
                    throw new Exception($"I Cant Find meal : {mealId}");

                var dishes = await _context.Dishes.AsNoTracking().Where(x => x.MealId == mealId).ToListAsync();
                var resp = new List<MealResponse>
                {
                    new MealResponse()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime,
                        userId = meal.UserId,
                        meal = new PythModels.PythMeal
                        {
                            description = meal.Description,
                            totalWeight = meal.Weight,
                            type = (mealtype)meal.Type,
                            food = dishes.Where(x => x.MealId == meal.Id).Select(x => new PythModels.PythFood()
                            {
                                description = x.Description,
                                weight = x.Weight,
                                nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein, x.Kkal)
                            }).ToList()
                        }
                    }
                };
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponse(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMealById Error", ex);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GetMealResponse>> GetUserMeals(GetUserMealsRequest req)
        {
            try
            {
                var resp = await _mealHelper.GetMeals(req);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponse(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMeals Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpPost]
        public async Task<ActionResult<List<MealResponse>>> GetUserMealsForAnal(long userTgId)
        {
            try
            {
                var resp = await _mealHelper.GetMealsForAnal(userTgId);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMealsForAnal Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {userTgId}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpPost]
        public async Task<ActionResult<GetMealKKResponse>> GetUserMealsKK(GetUserMealsRequest req)
        {
            try
            {
                var userId = (await _context.Users.SingleOrDefaultAsync(x => x.TgId == req.userTgId)).Id;
                var usi = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == userId);

                var resp = await _mealHelper.GetMealsKK(req);
                Dictionary<DateTime, List<PythMeal>> respd = resp
            .GroupBy(x => x.date.Date) // Группируем по дате
            .ToDictionary(
                g => g.Key, // Ключ словаря — дата
                g => g.Select(x => x.meal).ToList() // Значение — список meal
            );
                var realResp = new GetMealKKResponse(req.period, usi.Timeslide ?? 0);

                Dictionary<string, string> userInfo;
                if (string.IsNullOrEmpty(usi?.Extra))
                    userInfo = new Dictionary<string, string> { { "isempty", "true" } };
                else
                    userInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(usi.Extra);

                realResp.user_info = userInfo;
                decimal avarageKK = 0m;

                foreach (var day in realResp.days)
                {

                    var k = respd.GetValueOrDefault(day.date);
                    if (k != null)
                    {
                        var totalKk = k.SelectMany(meal => meal.food)
               .Sum(food => food.nutritional_value.kcal);
                        day.isEmpty = false;
                        day.Meals = k;
                        day.totalKK = totalKk;
                        avarageKK += totalKk;
                    }
                }
                var notEmptyDays = realResp.days.Where(x => x.totalKK != 0m).Count();
                if (notEmptyDays > 0)
                    realResp.total_avg_period = avarageKK / notEmptyDays;
                return Ok(realResp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMealsKK Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GetMealResponseV2>> GetSingleUserMeal(GetUserMealsRequest req)
        {
            try
            {
                var resp = await _mealHelper.GetSingleMeal(req);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponseV2(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetSingleUserMeal Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {req}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<GetWeekMealStatusResponse>>> GetUserWeekMealsStatus(long userTgId)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");

                var startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).AddDays(-7).Date;
                var meals = await _context.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
                var dishes = await _context.Dishes.AsNoTracking().Where(x => meals.Select(x => x.Id).Contains(x.MealId)).ToListAsync();
                var resp = new List<GetWeekMealStatusResponse>();
                for (var i = 1; i <= 7; i++)
                {
                    var ndate = startDate.AddDays(i);
                    var daymeals = new List<MealStatus>();
                    foreach (mealtype t in Enum.GetValues(typeof(mealtype)))
                    {
                        daymeals.Add(new MealStatus
                        {
                            Type = t,
                            isEmpty = !meals.Any(x => x.MealTime.Date == ndate && x.Type == (short)t)
                        });
                    }
                    var mlIds = meals.Where(x => x.MealTime.Date == ndate).Select(x => x.Id).ToList();
                    var isEptyDay = !daymeals.Any(x => !x.isEmpty);
                    resp.Add(new GetWeekMealStatusResponse
                    {

                        DisplayDay = ndate.ToString("dd.MM.yyyy"),
                        MealStatus = daymeals,
                        isEmpty = isEptyDay,
                        TotalKkal = isEptyDay ? 0.0m : dishes.Where(x => mlIds.Contains(x.MealId)).Select(x => x.Kkal).ToList().Sum()

                    }
                        );
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserWeekMealsStatus Error", ex);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetMealTotalResponse>> GetUserMealsTotal(long userTgId, Periods period)
        {
            try
            {
                var resp = await _mealHelper.GetMealTotal(userTgId, period);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess("GetUserMealsTotal Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateMealFromUnicode(string request)
        {
            try
            {
                request = request.Replace("\\\"", "\"");
                request = Regex.Unescape(request);
                var inp = Newtonsoft.Json.JsonConvert.DeserializeObject<EditMealRequest>(request);
                var res = await _mealHelper.CreateMeal(inp);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        #endregion

        #region User
        [Obsolete]
        [HttpGet]
        public async Task<ActionResult<GetMealResponse>> EnsureUser(long userTgId, string userName)
        {
            try
            {
                _logger.LogWarning($"User \n:userTgId={userTgId} userName={userName}");
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user != null)
                    return Ok(true);
                bool isActive = false;
                var subscription = await _context.Subscriptions.SingleOrDefaultAsync(x => x.UserTgId == userTgId && x.IsActive == true && x.IsLinked == false);
                if (subscription != null)
                {
                    isActive = true;
                    subscription.IsLinked = true;
                }
                await _context.Users.AddAsync(new DbModels.User
                {
                    TgId = userTgId,
                    StageId = 0,
                    LessonId = 0,
                    RegistrationTime = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime().AddHours(3)),
                    IsActive = isActive,
                    Username = string.IsNullOrEmpty(userName) ? null : userName,
                });
                if (subscription != null)
                {
                    _context.Subscriptions.Update(subscription);
                }
                await _context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess($"Упали при создании пользователя {userTgId}", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetMealResponse>> EnsureUserH(long userTgId, string userName)
        {
            try
            {
                _logger.LogWarning($"User \n:userTgId={userTgId} userName={userName}");
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user != null)
                    return Ok(true);
                bool isActive = false;
                var subscription = await _context.Subscriptions.SingleOrDefaultAsync(x => x.UserTgId == userTgId && x.IsActive == true && x.IsLinked == false);
                if (subscription != null)
                {
                    isActive = true;
                    subscription.IsLinked = true;
                }
                await _context.Users.AddAsync(new DbModels.User
                {
                    TgId = userTgId,
                    StageId = 0,
                    LessonId = 0,
                    RegistrationTime = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime().AddHours(3)),
                    IsActive = isActive,
                    Username = string.IsNullOrEmpty(userName) ? null : userName,
                });
                if (subscription != null)
                {
                    _context.Subscriptions.Update(subscription);
                }
                await _context.SaveChangesAsync();
                IntegratorHelper integratorHelper = new IntegratorHelper();
                var res = await integratorHelper.SendRequestAsync(new BotStartRequest { Email = string.Empty, FirstName = string.Empty, LastName = string.Empty, TgId = userTgId, Username = userName, start_text = "/start" });

                return Ok(true);
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess($"Упали при создании пользователя {userTgId}", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<bool>> GetUserWeekPlotH(long userTgId)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var goalkk = (await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == user.Id))?.Goalkk;

                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
                DateTime startDate = now.AddDays(-7).Date;
                int daysinperiod = 0;

                daysinperiod = now.Day - startDate.Day;
                var meals = await _context.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
                var dishes = await _context.Dishes.AsNoTracking().Where(x => meals.Select(x => x.Id).Contains(x.MealId)).ToListAsync();
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
                if (values.Any(x => x > 0))
                {
                    await _plotHelper.SendPlotH(values, labels, userTgId, goalkk);
                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess("GetUserWeekPlot Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        private decimal? ParseDecimal(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return null;
        }


        private async Task<ActionResult<bool>> AddUserExtraInfo(AddUserExtraRequest req)
        {
            try
            {
                _logger.LogInformation($"User \n:userTgId={req.UserTgId} add userInfo in AddUserExtraInfo ");
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == req.UserTgId);
                var userId = user.Id;
                var usi = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == userId);
                var info = Newtonsoft.Json.JsonConvert.SerializeObject(req.Info);

                short? age = string.IsNullOrEmpty(req.Info["user_info_age"]) ? null : short.Parse(req.Info["user_info_age"]);
                decimal? weight = ParseDecimal(req.Info["user_info_weight"]);
                decimal? height = ParseDecimal(req.Info["user_info_height"]);
                decimal? goalkk = ParseDecimal(req.Info["target_calories"]);
                decimal? timeslide = ParseDecimal(req.Info["user_info_timeslide"]);


                string gender = string.IsNullOrEmpty(req.Info["user_info_gender"]) ? null : req.Info["user_info_gender"];

                bool IsmorningPing = string.IsNullOrEmpty(req.Info["user_info_morning_ping"]) ? false : TimeOnly.TryParseExact(req.Info["user_info_morning_ping"], "%H:mm", out var m);
                //bool IseveningPing = string.IsNullOrEmpty(req.Info["user_info_evening_ping"]) ? false : TimeOnly.TryParseExact(req.Info["user_info_evening_ping"], "%H:mm", out var e);


                double eh = 0;
                double gh = 0;
                decimal wc = 0;
                bool Isgymhrs = string.IsNullOrEmpty(req.Info["user_info_gym_hrs"]) ? false : double.TryParse(req.Info["user_info_gym_hrs"], out gh);
                bool Isexcersisehrs = string.IsNullOrEmpty(req.Info["user_info_excersise_hrs"]) ? false : double.TryParse(req.Info["user_info_excersise_hrs"], out eh);
                bool Isweightchange = string.IsNullOrEmpty(req.Info["user_info_weight_change"]) ? false : decimal.TryParse(req.Info["user_info_weight_change"], out wc);

                string goal = string.IsNullOrEmpty(req.Info["user_info_goal"]) ? null : req.Info["user_info_goal"];

                if (usi == null)
                {
                    await _context.Userinfos.AddAsync(new Userinfo
                    {
                        UserId = userId,
                        Extra = info,
                        Age = age,
                        Weight = weight,
                        Height = height,
                        Gender = gender,
                        Goalkk = goalkk,
                        MorningPing = IsmorningPing ? m : null,
                        //EveningPing = IseveningPing ? e : null,
                        Timeslide = timeslide,
                        TgId = req.UserTgId,
                        Goal = goal
                    });
                }
                else
                {
                    usi.Extra = info;
                    usi.Age = age;
                    usi.Weight = weight;
                    usi.Height = height;
                    usi.Gender = gender;
                    usi.Goalkk = goalkk;
                    usi.MorningPing = IsmorningPing ? m : null;
                    //usi.EveningPing = IseveningPing ? e : null;
                    usi.Timeslide = timeslide;
                    usi.Goal = goal;
                    _context.Update(usi);
                }
                if (timeslide != null)
                {
                    user.Timeslide = (decimal)timeslide;
                    _context.Update(user);
                }
                await _context.SaveChangesAsync();
                IntegratorHelper integratorHelper = new IntegratorHelper();
                var res = await integratorHelper.SendRequestAsync(new ProfileAddRequest { Email = user.Email, ProfileName = user.Username, FirstName = user.Username, LastName = string.Empty, TgId = user.TgId, Username = user.Username, Age = age ?? 0, DailyCaloricNormKcal = goalkk ?? 0, Gender = gender, Goal = goal, HeightCm = height ?? 0, WeightKg = weight ?? 0, MacronutrientNormG = string.Empty, TargetWeightKg = Isweightchange ? weight ?? 0 + wc : 0, WeeklyActivityHours = Isgymhrs && Isexcersisehrs == true ? (eh + gh * 1.5) : 0 });

                if (IsmorningPing)
                    await _taskSchedulerService.TimerRestart();
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("AddUserExtraInfo Error", ex);
                await ErrorHelper.SendErrorMess($"Input: {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<bool>> AddOrUpdateUserExtraInfo(AddUserExtraRequest req)
        {
            try
            {
                _logger.LogInformation($"User \n:userTgId={req.UserTgId} add userInfo in AddOrUpdateUserExtraInfo {Newtonsoft.Json.JsonConvert.SerializeObject(req)}");

                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == req.UserTgId);
                var userId = user.Id;
                var usi = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == userId);
                bool ismorningPing = false;
                if (usi == null)
                {
                    return await AddUserExtraInfo(req);
                    //return Problem("У пользователя нет доп информации");
                }
                else
                {
                    if (usi.Extra == null)
                        usi.Extra = Newtonsoft.Json.JsonConvert.SerializeObject(req.Info);
                    else
                    {
                        var dbInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(usi.Extra);

                        foreach (var kv in req.Info)
                        {
                            if (dbInfo.ContainsKey(kv.Key))
                                dbInfo[kv.Key] = kv.Value;
                            else
                                dbInfo.Add(kv.Key, kv.Value);
                        }

                        usi.Extra = Newtonsoft.Json.JsonConvert.SerializeObject(dbInfo);
                    }
                    short? age = req.Info.ContainsKey("user_info_age") == true ? (string.IsNullOrEmpty(req?.Info["user_info_age"]) ? null : short.Parse(req.Info["user_info_age"])) : null;
                    if (age != null)
                        usi.Age = age;

                    decimal? weight = req.Info.ContainsKey("user_info_weight") == true ? ParseDecimal(req.Info["user_info_weight"]) : null;
                    if (weight != null)
                        usi.Weight = weight;

                    decimal? height = req.Info.ContainsKey("user_info_height") == true ? ParseDecimal(req.Info["user_info_height"]) : null;
                    if (height != null)
                        usi.Height = height;

                    string gender = req.Info.ContainsKey("user_info_gender") == true ? (string.IsNullOrEmpty(req.Info["user_info_gender"]) ? null : req.Info["user_info_gender"]) : null;
                    if (gender != null)
                        usi.Gender = gender;

                    decimal? goalkk = req.Info.ContainsKey("target_calories") == true ? ParseDecimal(req.Info["target_calories"]) : null;
                    if (goalkk != null)
                        usi.Goalkk = goalkk;

                    ismorningPing = req.Info.ContainsKey("user_info_morning_ping") == true ? (string.IsNullOrEmpty(req.Info["user_info_morning_ping"]) ? false : TimeOnly.TryParseExact(req.Info["user_info_morning_ping"], "HH:mm", out var m)) : false;
                    if (ismorningPing)
                        usi.MorningPing = m;

                    decimal? timeslide = req.Info.ContainsKey("user_info_timeslide") == true ? ParseDecimal(req.Info["user_info_timeslide"]) : null;
                    if (timeslide != null)
                    {
                        usi.Timeslide = timeslide;
                        user.Timeslide = (decimal)timeslide;
                    }

                    string goal = req.Info.ContainsKey("user_info_goal") == true ? (string.IsNullOrEmpty(req.Info["user_info_goal"]) ? null : req.Info["user_info_goal"]) : null;
                    if (goal != null)
                        usi.Goal = goal;


                    _context.Update(usi);
                }
                await _context.SaveChangesAsync();
                if (ismorningPing)
                    await _taskSchedulerService.TimerRestart();
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("AddOrUpdateUserExtraInfo Error", ex);
                await ErrorHelper.SendErrorMess($"Input={Newtonsoft.Json.JsonConvert.SerializeObject(req)}");
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<bool>> AddUserLesson(long UserTgId, int lesson)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == UserTgId);
                var userId = user.Id;
                var usi = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == userId);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide));
                if (usi == null)
                {
                    await _context.Userinfos.AddAsync(new Userinfo
                    {
                        UserId = userId,
                        Donelessonlist = lesson.ToString(),
                        LastlessonTime = now,
                    });
                }
                else
                {
                    usi.LastlessonTime = now;
                    if (usi.Donelessonlist == null)
                        usi.Donelessonlist = $"{lesson}";
                    else
                    if (lesson == 99 && usi.Donelessonlist.Contains($"{lesson}"))
                        return Ok(true);
                    if (usi.Donelessonlist.Contains($",{lesson},")
                             || usi.Donelessonlist.Substring(usi.Donelessonlist.Length - lesson.ToString().Length) == lesson.ToString())
                    { }
                    else
                    {
                        usi.Donelessonlist += $",{lesson}";
                        _context.Update(usi);
                    }
                }
                IntegratorHelper integratorHelper = new IntegratorHelper();
                IntegratorResponse res;
                if (lesson == 21)
                    res = await integratorHelper.SendRequestAsync(new LessonEndRequest { Email = user.Email, ProfileName = user.Username, FirstName = user.Username, LastName = string.Empty, TgId = user.TgId, Username = user.Username, LessonsCompleted = lesson, LessonsCurrent = lesson + 1 });
                else
                    res = await integratorHelper.SendRequestAsync(new AllLessonsEndRequest { Email = user.Email, ProfileName = user.Username, FirstName = user.Username, LastName = string.Empty, TgId = user.TgId, Username = user.Username, LessonsCompleted = lesson });

                await _context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("AddUserLesson Error", ex);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<bool>>> GetUserLessons(long UserTgId)
        {
            try
            {
                var userId = (await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == UserTgId)).Id;
                var usi = await _context.Userinfos.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
                //var res = new List<(string, bool)>();
                var res2 = new List<bool>();
                List<string> usisplit = new List<string>();
                if (!String.IsNullOrEmpty(usi?.Donelessonlist))
                {
                    usisplit = usi.Donelessonlist.Split(',').ToList();
                }
                // res.Add(new("99", usisplit.Contains("99") ? true : false));
                res2.Add(usisplit.Contains("99") ? true : false);
                for (var i = 1; i < 22; i++)
                {
                    //res.Add(new(i.ToString(), usisplit.Contains(i.ToString()) ? true : false));
                    res2.Add(usisplit.Contains(i.ToString()) ? true : false);
                }
                //var resString = "{";
                //foreach (var el in res)
                //{
                //    resString += $"{el.Item1}:{el.Item2},";
                //}
                //resString = resString.Remove(resString.Length - 1, 1);
                //resString += "}";
                return Ok(res2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<int>> GetLastUserLesson(long UserTgId)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == UserTgId);
                var usi = await _context.Userinfos.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user.Id);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;

                if (usi == null)
                {
                    return 0;
                }
                else
                {
                    var les = int.Parse(usi.Donelessonlist.Split(',').ToList().Last());
                    if (now.Date > usi.LastlessonTime.Value.Date)
                        return les;
                    else
                    {
                        if (les == 99 || les == 21)
                            return les;
                        else
                            return (les - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendErrorMess($"GetLastUserLesson Error. User={UserTgId}", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetUserPingResponse>> GetUserPing(long UserTgId, short TimeOfDay)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == UserTgId);
                var userId = user.Id;
                var usi = await _context.Userinfos.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide));
                if (usi.MorningPing == null || usi.EveningPing == null || usi.Timeslide == null)
                    return new GetUserPingResponse { MskTime = null };
                DateTime ping = now;
                if (TimeOfDay == 0)//утро
                {
                    ping = DateOnly.FromDateTime(now).ToDateTime((TimeOnly)usi.MorningPing);
                }
                else if (TimeOfDay == 1)//вечер
                {
                    ping = DateOnly.FromDateTime(now).ToDateTime((TimeOnly)usi.EveningPing);
                }

                var slicePing = ping.AddHours(double.Parse(usi.Timeslide.ToString()));
                var correct = (slicePing.Date - ping.Date).TotalDays;
                return new GetUserPingResponse { MskTime = slicePing.ToString("HH:mm"), DayCorrection = (short)correct };
                //$"{slicePing.Hour}:{slicePing.Minute}"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetUserPingResponse>> GetCustomPing(long UserTgId, string TimeToSlide)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == UserTgId);
                var userId = user.Id;
                var usi = await _context.Userinfos.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide));
                if (usi.Timeslide == null)
                    return new GetUserPingResponse { MskTime = null };

                var isparse = TimeOnly.TryParseExact(TimeToSlide, "HH:mm", out var timetoslide);
                if (!isparse)
                    return new GetUserPingResponse { MskTime = null };

                var ping = DateOnly.FromDateTime(now).ToDateTime(timetoslide);


                var slicePing = ping.AddHours(double.Parse(usi.Timeslide.ToString()));
                var correct = (slicePing.Date - ping.Date).TotalDays;
                return new GetUserPingResponse { MskTime = slicePing.ToString("HH:mm"), DayCorrection = (short)correct };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<ActionResult<Dictionary<string, string>>> GetUserExtraInfo(long userTgId)
        {
            try
            {
                var userId = (await _context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == userTgId)).Id;
                var usi = await _context.Userinfos.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
                if (string.IsNullOrEmpty(usi?.Extra))
                    return Ok(new Dictionary<string, string> { { "isempty", "true" } });
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(usi.Extra);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public async Task<bool> CheckSubscription(long UserTgId)
        {
            //return  _context.Subscriptions.Any(x => x.UserTgId == UserTgId && x.IsLinked == true && x.IsActive == true);
            return _context.Users.AsNoTracking().Any(x => x.TgId == UserTgId && x.IsActive == true);
        }

        [HttpPost]
        public async Task<bool> SetNotifyStatus(long UserTgId, bool status)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == UserTgId);
                user.NotifyStatus = status;
                _context.Update(user);
                await _context.SaveChangesAsync();
                await _taskSchedulerService.TimerRestart();
                return true;
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("Упали на изменении подписки на уведомления", ex);
                return false;
            }
        }
        #endregion

        [HttpPost]
        public async Task<bool> ReloadNotify()
        {
            try
            {
                await _taskSchedulerService.TimerRestart();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReloadNotify");
                await ErrorHelper.SendErrorMess("ReloadNotify", ex);
                return false;
            }
        }

        [HttpGet]
        public async Task<List<string>> GetTimers()
        {
            return await _taskSchedulerService.GetTimers();
        }
        #endregion


        [HttpPost]
        public async Task<bool> DeleteUser(long userTgId)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                var user = await _context.Users.SingleAsync(x => x.TgId == userTgId);
                var gpts = _context.Gptrequests.Where(x => x.UserTgid == userTgId).ToList();
                var userinfo = await _context.Userinfos.SingleOrDefaultAsync(x => x.UserId == user.Id);
                var meals = _context.Meals.Where(x => x.UserId == user.Id).ToList();
                var mealIds = meals.Select(x => x.Id);
                var dishes = _context.Dishes.Where(x => mealIds.Contains(x.MealId));
                //var subs = _context.Subscriptions.Where(x => x.UserId == user.Id);
                _context.Dishes.RemoveRange(dishes);
                _context.Meals.RemoveRange(meals);
                if (userinfo != null)
                    _context.Userinfos.RemoveRange(userinfo);
                _context.Users.Remove(user);
                _context.Gptrequests.RemoveRange(gpts);
                //_context.Subscriptions.RemoveRange(subs);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await ErrorHelper.SendSystemMess($"Удалили пользователя {userTgId}");
                return true;
            }
            catch (Exception ex)
            {
                await ErrorHelper.SendSystemMess($"Ошибка удаления пользователя {userTgId}");
                return false;
            }
        }

        //[HttpGet]
        //public async Task<bool> SendManualNotify(int userId, bool isMorning)
        //{
        //    try
        //    {
        //        await _notificationHelper.SendNotification(userId, isMorning);
        //        return true;
        //    }
        //    catch (Exception ex) { return false; }
        //}

        //[HttpGet]
        //public async Task<bool> SendManualNotifyH(long userTgId, bool isMorning)
        //{
        //    try
        //    {
        //        var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
        //        await _notificationHelper.SendNotificationH(new UserPing { UserTgId = userTgId, UserId = user.Id }, isMorning);
        //        return true;
        //    }
        //    catch (Exception ex) { return false; }
        //}

        [HttpGet]
        public async Task<bool> SendManualNotifyNew(long userTgId)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
                await _notificationHelper.SendNotificationSingle(new UserPing { UserTgId = userTgId, UserId = user.Id });
                return true;
            }
            catch (Exception ex) { return false; }
        }
        [HttpGet]
        public async Task<bool> SendManualMessToUserH(long userTgId, string mess)
        {
            try
            {
                await _notificationHelper.SendCustomMessToUserH(userTgId, mess);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        [HttpPost]
        public async Task<ActionResult<List<long>>> GetUsersIds(bool onlyUs, bool onlyActive)
        {
            try
            {
                if (onlyUs)
                    return Ok(new List<long>() { 389054202l, 464682207l });
                var users = await _context.Users.AsNoTracking().ToListAsync();
                if (onlyActive)
                    users.RemoveAll(x => x.IsActive == false);
                return Ok(users.Select(x => x.TgId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMealsForAnal Error", ex);

                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public async Task<bool> StartVoteNotify()
        {
            var usersTgIds = await _context.Users.Where(x => x.IsActive == true).Select(x => x.TgId).ToListAsync();

            _ = Task.Run(async () =>
            {
                List<long> doneids = new List<long>();
                try
                {
                    await _notificationHelper.SendAlertToMe("Start Vote");
                  
                    foreach (var userTgId in usersTgIds)
                    {
                        try
                        {
                            await _notificationHelper.SendVoteNotificationSingle(userTgId);
                            doneids.Add(userTgId);
                        }
                        catch (Exception ex)
                        {
                            await _notificationHelper.SendAlertToMe($"Exception on Vote id= {userTgId}");
                        }
                    }
                    await _notificationHelper.SendAlertToMe($"End Vote. Total={doneids.Count}");
                }
                catch (Exception ex)
                {
                    await _notificationHelper.SendAlertToMe("Exception on Vote");
                    _logger.LogError(ex, $"Exception on Vote:");
                    _logger.LogInformation($"DoneList={Newtonsoft.Json.JsonConvert.SerializeObject(doneids)}");
                    await _notificationHelper.SendAlertToMe("Exception on Vote:");
                }
            });

            return true;
        }

        [HttpGet]
        public async Task<bool> SetUserVote(long userTgId, short vote)
        {
            try
            {
                var userInfo = await _context.Userinfos.SingleOrDefaultAsync(x => x.TgId == userTgId);
                if (userInfo == null)
                    return false;
                userInfo.Vote = vote;
                _context.Userinfos.Update(userInfo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await ErrorHelper.SendErrorMess("GetUserMealById Error", ex);
                return false;
            }
        }
        //[HttpPost]
        //public async Task<bool> Test()
        //{
        //    IntegratorHelper integratorHelper = new IntegratorHelper();
        //    var res = await integratorHelper.SendRequestAsync(new BotStartRequest { Email = "xren@mail.vam", FirstName = "H", LastName = "I", start_text = "/start", TgId = 397597158, Username = "@aleshenka93" });
        //return res.Status;
        //}
        //[HttpPost]
        //public async Task<bool> SaveRate(long tgid, short rating)
        //{
        //    try
        //    {
        //        _logger.LogError($"tg={tgid},rate={rating}");
        //        await _context.Promos.AddAsync(new Promo { PromoCode = tgid.ToString(), Discount = rating });
        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return false;
        //    }
        //}
    }
}
