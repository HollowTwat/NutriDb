using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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
        public TypesCRUDController(railwayContext context, MealHelper mealHelper, ILogger<TypesCRUDController> logger, PlotHelper plotHelper)
        {
            _context = context;
            _mealHelper = mealHelper;
            _logger = logger;
            _plotHelper = plotHelper;
        }

        #region AllCRUDS

        #region Promo
        [HttpGet]
        public ActionResult<IEnumerable<Promo>> GetAllPromo()
        {
            try
            {
                var res = _context.Promos.ToList();
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        #endregion

        #region Meal
        [HttpPost]
        public int CreateMeal(EditMealRequest request)
        {
            try
            {
                var mealId = _mealHelper.CreateMeal(request);
                _logger.LogWarning($"UserTG={request.userTgId} Meal={mealId} was added");
                return mealId;
                // return Ok(res);
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("Meal Create Error", ex);
                _logger.LogError(ex, "Meal Create Error");
                return 0;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public bool EditMeal(EditMealRequest request)
        {
            try
            {
                var res = _mealHelper.EditMeal(request);
                _logger.LogWarning($"User={request.userTgId} Meal={res} was edited");
                return true;
                // return Ok(res);
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("Meal Edit Error", ex);
                _logger.LogError(ex, "Meal Edit Error");
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public bool DeleteMeal(int mealId, long userTgId)
        {
            try
            {
                var res = _mealHelper.DeleteMeal(mealId, userTgId);
                if (res != 0)
                {
                    _logger.LogWarning($"User={userTgId} Meal={res} was deleted");
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
                ErrorHelper.SendErrorMess("Meal Delete Error", ex);
                _logger.LogError(ex, "Meal Edit Error");
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [Obsolete]
        [HttpGet]
        public ActionResult<GetMealResponse> GetTodayUserMeals(long userTgId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var meals = _context.Meals.Where(x => x.UserId == user.Id && DateTime.Equals(x.MealTime.Date, DateTime.UtcNow.ToLocalTime().AddHours(3).Date)).ToList();

                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<MealResponse>() { };
                foreach (var meal in meals)
                {
                    resp.Add(new MealResponse()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime,
                        userId = meal.UserId,
                        meal = new PythModels.PythMeal
                        {
                            description = meal.Description,
                            totalWeight = meal.Weight,
                            type = (mealtype)meal.Type,
                            food = dishes.Where(x => x.MealId == meal.Id).ToList().Select(x => new PythModels.PythFood()
                            {
                                description = x.Description,
                                weight = x.Weight,
                                nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein, x.Kkal)
                            }).ToList()
                        }
                    }
                    );
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponse(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<GetMealResponse> GetUserMealById(long userTgId, long mealId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var meal = _context.Meals.SingleOrDefault(x => x.UserId == user.Id && x.Id == mealId);
                if (meal == null)
                    throw new Exception($"I Cant Find meal : {mealId}");

                var dishes = _context.Dishes.Where(x => x.MealId == mealId);
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
                            food = dishes.Where(x => x.MealId == meal.Id).ToList().Select(x => new PythModels.PythFood()
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
                ErrorHelper.SendErrorMess("GetUserMealById Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<GetMealResponse> GetUserMeals(GetUserMealsRequest req)
        {
            try
            {
                var resp = _mealHelper.GetMeals(req);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponse(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<GetMealResponseV2> GetSingleUserMeal(GetUserMealsRequest req)
        {
            try
            {
                var resp = _mealHelper.GetSingleMeal(req);
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResponseV2(resp)));
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("GetSingleUserMeal Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<List<GetWeekMealStatusResponse>> GetUserWeekMealsStatus(long userTgId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");

                var startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToList();
                var dishes = _context.Dishes.Where(x => meals.Select(x => x.Id).ToList().Contains(x.MealId));
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

                        DisplayDay = ndate.ToString("dd.MM"),
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
                ErrorHelper.SendErrorMess("GetUserWeekMealsStatus Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<GetMealTotalResponse> GetUserMealsTotal(long userTgId, Periods period)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");

                DateTime startDate = DateTime.UtcNow.Date;
                int daysinperiod = 0;
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).Date;
                switch (period)
                {
                    case Periods.day:
                        startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1).Date;
                        break;
                    case Periods.week:
                        startDate = MealHelper.GetFirstDayOfWeek(now);
                        break;
                    case Periods.mathweek:
                        startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                        break;
                    case Periods.math3weeks:
                        startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-21).Date;
                        break;
                    case Periods.month:
                        startDate = new DateTime(now.Year, now.Month, 1);
                        break;
                }
                daysinperiod = (now - startDate).Days;
                var mealsIds = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsIds.Contains(x.MealId)).ToList();
                var resp = new GetMealTotalResponse();
                foreach (var dish in dishes)
                {

                    resp.TotalCarbs += dish.Carbs;
                    resp.TotalProt += dish.Protein;
                    resp.TotalFats += dish.Fats;
                    resp.TotalKkal += dish.Kkal;
                }
                var extra = _context.Userinfos.SingleOrDefault(x => x.UserId == user.Id).Extra;
                if (extra == null) { resp.GoalKkal = 0.0m; }
                else
                {
                    var extraDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(extra);
                    resp.GoalKkal = decimal.Parse(extraDict["target_calories"]) * daysinperiod;
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("GetUserMealsTotal Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<int> CreateMealFromUnicode(string request)
        {
            try
            {
                request = request.Replace("\\\"", "\"");
                request = Regex.Unescape(request);
                var inp = Newtonsoft.Json.JsonConvert.DeserializeObject<EditMealRequest>(request);
                var res = _mealHelper.CreateMeal(inp);
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
        [HttpGet]
        public ActionResult<GetMealResponse> EnsureUser(long userTgId, string userName)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user != null)
                    return Ok(true);
                _context.Users.Add(new DbModels.User
                {
                    TgId = userTgId,
                    Timezone = 0,
                    StageId = 0,
                    LessonId = 0,
                    RegistrationTime = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime().AddHours(3)),
                    IsActive = true,
                    Username = string.IsNullOrEmpty(userName) ? null : userName,
                });
                _context.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<bool> GetUserWeekPlot(long userTgId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var goalkk = _context.Userinfos.SingleOrDefault(x => x.UserId == user.Id).Goalkk;
                DateTime startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                int daysinperiod = 0;
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3).Date;

                daysinperiod = now.Day - startDate.Day;
                var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToList();
                var dishes = _context.Dishes.Where(x => meals.Select(x => x.Id).Contains(x.MealId)).ToList();
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
                    _plotHelper.SendPlot(values, labels, userTgId, goalkk);
                return Ok(true);
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("GetUserWeekPlot Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }


        [HttpPost]
        public ActionResult<bool> AddUserExtraInfo(AddUserExtraRequest req)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == req.UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                var info = Newtonsoft.Json.JsonConvert.SerializeObject(req.Info);

                short? age = string.IsNullOrEmpty(req.Info["user_info_age"]) ? null : short.Parse(req.Info["user_info_age"]);
                decimal? weight = string.IsNullOrEmpty(req.Info["user_info_weight"]) ? null : decimal.Parse(req.Info["user_info_weight"]);
                decimal? height = string.IsNullOrEmpty(req.Info["user_info_height"]) ? null : decimal.Parse(req.Info["user_info_height"]);
                string gender = string.IsNullOrEmpty(req.Info["user_info_gender"]) ? null : req.Info["user_info_gender"];
                decimal? goalkk = string.IsNullOrEmpty(req.Info["target_calories"]) ? null : decimal.Parse(req.Info["target_calories"]);

                string morningPing = string.IsNullOrEmpty(req.Info["user_info_morning_ping"]) ? null : TimeOnly.TryParseExact(req.Info["user_info_morning_ping"], "HH:mm", out var m) == true ? req.Info["user_info_morning_ping"] : null;
                string eveningPing = string.IsNullOrEmpty(req.Info["user_info_evening_ping"]) ? null : TimeOnly.TryParseExact(req.Info["user_info_evening_ping"], "HH:mm", out var e) == true ? req.Info["user_info_evening_ping"] : null;
                decimal? timeslide = string.IsNullOrEmpty(req.Info["user_info_timeslide"]) ? null : decimal.Parse(req.Info["user_info_timeslide"]);

                string goal = string.IsNullOrEmpty(req.Info["user_info_goal"]) ? null : req.Info["user_info_goal"];


                if (usi == null)
                {
                    _context.Userinfos.Add(new Userinfo
                    {
                        UserId = userId,
                        Extra = info,
                        Age = age,
                        Weight = weight,
                        Height = height,
                        Gender = gender,
                        Goalkk = goalkk,
                        MorningPing = morningPing,
                        EveningPing = eveningPing,
                        Timeslide = timeslide,
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
                    usi.MorningPing = morningPing;
                    usi.EveningPing = eveningPing;
                    usi.Timeslide = timeslide;
                    usi.Goal = goal;
                    _context.Update(usi);
                }
                _context.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("AddUserExtraInfo Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<bool> AddOrUpdateUserExtraInfo(AddUserExtraRequest req)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == req.UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);



                if (usi == null)
                {
                    return Problem("У пользователя нет доп информации");
                }
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


                    short? age = req.Info.ContainsKey("user_info_age") == true ? (string.IsNullOrEmpty(req?.Info["user_info_age"]) ? null : short.Parse(req.Info["user_info_age"])) : null;
                    if (age != null)
                        usi.Age = age;

                    decimal? weight = req.Info.ContainsKey("user_info_weight") == true ? (string.IsNullOrEmpty(req.Info["user_info_weight"]) ? null : decimal.Parse(req.Info["user_info_weight"])) : null;
                    if (weight != null)
                        usi.Weight = weight;

                    decimal? height = req.Info.ContainsKey("user_info_height") == true ? (string.IsNullOrEmpty(req.Info["user_info_height"]) ? null : decimal.Parse(req.Info["user_info_height"])) : null;
                    if (height != null)
                        usi.Height = height;

                    string gender = req.Info.ContainsKey("user_info_gender") == true ? (string.IsNullOrEmpty(req.Info["user_info_gender"]) ? null : req.Info["user_info_gender"]) : null;
                    if (gender != null)
                        usi.Gender = gender;

                    decimal? goalkk = req.Info.ContainsKey("target_calories") == true ? (string.IsNullOrEmpty(req.Info["target_calories"]) ? null : decimal.Parse(req.Info["target_calories"])) : null;
                    if (goalkk != null)
                        usi.Goalkk = goalkk;

                    string morningPing = req.Info.ContainsKey("user_info_morning_ping") == true ? (string.IsNullOrEmpty(req.Info["user_info_morning_ping"]) ? null : TimeOnly.TryParseExact(req.Info["user_info_morning_ping"], "HH:mm", out var m) == true ? req.Info["user_info_morning_ping"] : null) : null;
                    if (morningPing != null)
                        usi.MorningPing = morningPing;

                    string eveningPing = req.Info.ContainsKey("user_info_evening_ping") == true ? (string.IsNullOrEmpty(req.Info["user_info_evening_ping"]) ? null : TimeOnly.TryParseExact(req.Info["user_info_evening_ping"], "HH:mm", out var e) == true ? req.Info["user_info_evening_ping"] : null) : null;
                    if (eveningPing != null)
                        usi.EveningPing = eveningPing;

                    decimal? timeslide = req.Info.ContainsKey("user_info_timeslide") == true ? (req.Info.ContainsKey("user_info_timeslide") == true ? (string.IsNullOrEmpty(req.Info["user_info_timeslide"]) ? null : decimal.Parse(req.Info["user_info_timeslide"])) : null) : null;
                    if (timeslide != null)
                        usi.Timeslide = timeslide;

                    string goal = req.Info.ContainsKey("user_info_goal") == true ? (string.IsNullOrEmpty(req.Info["user_info_goal"]) ? null : req.Info["user_info_goal"]) : null;
                    if (goal != null)
                        usi.Goal = goal;


                    _context.Update(usi);
                }
                _context.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("AddOrUpdateUserExtraInfo Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<bool> AddUserLesson(long UserTgId, int lesson)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                if (usi == null)
                {
                    _context.Userinfos.Add(new Userinfo
                    {
                        UserId = userId,
                        Donelessonlist = lesson.ToString()
                    });
                }
                else
                {
                    if (usi.Donelessonlist == null)
                        usi.Donelessonlist = $"{lesson}";
                    else
                    if (!usi.Donelessonlist.Contains(lesson.ToString()))
                    {
                        usi.Donelessonlist += $",{lesson}";
                        _context.Update(usi);
                    }
                }
                _context.SaveChanges();
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<List<bool>> GetUserLessons(long UserTgId)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
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
        public ActionResult<GetUserPingResponse> GetUserPing(long UserTgId, short TimeOfDay)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3);
                if (!TimeOnly.TryParseExact(usi.MorningPing, "HH:mm", out var morningPing) || !TimeOnly.TryParseExact(usi.EveningPing, "HH:mm", out var eveningPing) || usi.Timeslide == null)
                    return new GetUserPingResponse { MskTime = null };
                DateTime ping = DateTime.UtcNow;
                if (TimeOfDay == 0)//утро
                {
                    ping = DateOnly.FromDateTime(now).ToDateTime(morningPing);
                }
                else if (TimeOfDay == 1)//вечер
                {
                    ping = DateOnly.FromDateTime(now).ToDateTime(eveningPing);
                }

                var slicePing = ping.AddHours(double.Parse(usi.Timeslide.ToString()));
                var correct = (slicePing - ping).TotalDays;
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
        public ActionResult<GetUserPingResponse> GetCustomPing(long UserTgId, string TimeToSlide)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                var now = DateTime.UtcNow.ToLocalTime().AddHours(3);
                if (usi.Timeslide == null)
                    return new GetUserPingResponse { MskTime = null };

                var isparse = TimeOnly.TryParseExact(TimeToSlide, "HH:mm", out var timetoslide);
                if (!isparse)
                    return new GetUserPingResponse { MskTime = null };

                var ping = DateOnly.FromDateTime(now).ToDateTime(timetoslide);


                var slicePing = ping.AddHours(double.Parse(usi.Timeslide.ToString()));
                var correct = (slicePing - ping).TotalDays;
                return new GetUserPingResponse { MskTime = slicePing.ToString("HH:mm"), DayCorrection = (short)correct };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<int> GetLastUserLesson(long UserTgId)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                if (usi == null)
                {
                    return 0;
                }
                else
                {
                    return int.Parse(usi.Donelessonlist.Split(',').ToList().Last());
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.SendErrorMess("GetLastUserLesson Error", ex);
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<Dictionary<string, string>> GetUserExtraInfo(long userTgId)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == userTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(usi.Extra);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        #endregion


        #endregion
    }
}
