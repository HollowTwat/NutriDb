using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.NoCodeModels;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
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
        public TypesCRUDController(railwayContext context, MealHelper mealHelper, ILogger<TypesCRUDController> logger)
        {
            _context = context;
            _mealHelper = mealHelper;
            _logger = logger;
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
                _logger.LogError(ex, "Meal Edit Error");
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [Obsolete]
        [HttpGet]
        public ActionResult<GetMealResp> GetTodayUserMeals(long userTgId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var meals = _context.Meals.Where(x => x.UserId == user.Id && DateTime.Equals(x.MealTime.Date, DateTime.UtcNow.ToLocalTime().AddHours(3).Date)).ToList();

                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<MealResp>() { };
                foreach (var meal in meals)
                {
                    resp.Add(new MealResp()
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
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResp(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpGet]
        public ActionResult<GetMealResp> GetUserMealById(long userTgId, long mealId)
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
                var resp = new List<MealResp>
                {
                    new MealResp()
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
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResp(resp)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        public ActionResult<GetMealResp> GetUserMeals(GetUserMealsRequest req)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == req.userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {req.userTgId}");
                //var inDay = (DayOfWeek)day;
                var startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                //var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
                var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToList();
                if (req.day != null)
                {
                    meals = meals.Where(x => x.MealTime.DayOfWeek == (DayOfWeek)req.day).ToList();
                }
                if (!String.IsNullOrEmpty(req.dayStr))
                {
                    meals = meals.Where(x => x.MealTime.Date == DateTime.ParseExact($"{req.dayStr}.{startDate.Year}", "dd.MM.yyyy", CultureInfo.InvariantCulture).Date).ToList();
                }
                if (req.typemeal != null)
                {
                    meals = meals.Where(x => x.Type == ((short)req.typemeal)).ToList();
                }
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<MealResp>() { };
                foreach (var meal in meals)
                {

                    resp.Add(new MealResp()
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
                            }).ToList(),
                        }
                    });
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(new GetMealResp(resp)));
            }
            catch (Exception ex)
            {
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
                for (var i = 0; i < 7; i++)
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
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
       private static DateTime GetFirstDayOfWeek(DateTime date)
        {
            DayOfWeek firstDay = DayOfWeek.Monday;
            int diff = (7 + (date.DayOfWeek - firstDay)) % 7;
            return date.AddDays(-1 * diff).Date;
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
                        daysinperiod = 1;
                        break;
                    case Periods.week:
                        startDate = GetFirstDayOfWeek(now);
                        daysinperiod = now.Day - startDate.Day;
                        break;
                    case Periods.month:
                        startDate = new DateTime(now.Year, now.Month, 1);
                        daysinperiod = now.Day - startDate.Day;
                        break;
                }

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
                    resp.GoalKkal = decimal.Parse(extraDict["target_calories"])* daysinperiod;
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }


        [HttpGet]
        public ActionResult<GetMealResp> EnsureUser(long userTgId, string userName)
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

        [HttpPost]
        public ActionResult<bool> AddUserExtraInfo(AddUserExtraRequest req)
        {
            try
            {
                var userId = _context.Users.SingleOrDefault(x => x.TgId == req.UserTgId).Id;
                var usi = _context.Userinfos.SingleOrDefault(x => x.UserId == userId);
                var info = Newtonsoft.Json.JsonConvert.SerializeObject(req.Info);
                if (usi == null)
                {
                    _context.Userinfos.Add(new Userinfo
                    {
                        UserId = userId,
                        Extra = info
                    });
                }
                else
                {
                    usi.Extra = info;
                    _context.Update(usi);
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
