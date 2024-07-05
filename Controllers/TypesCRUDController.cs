using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using System;
using System.Collections.Generic;
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
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        #endregion

        #region Meal
        [HttpPost]
        public bool CreateMeal(CreateMealRequest request)
        {
            try
            {
                var res = _mealHelper.CreateMeal(request);
                _logger.LogInformation($"UserTG={request.userTgId} Meal={res} was added");
                return true;
                // return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal Create Error");
                return false;
                //return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpPost]
        public bool EditMeal(CreateMealRequest request)
        {
            try
            {
                var res = _mealHelper.CreateMeal(request);
                _logger.LogInformation($"User={request.userTgId} Meal={res} was edited");
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
        [HttpGet]
        public ActionResult<List<GetMealResp>> GetTodayUserMeals(int userTgId)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var meals = _context.Meals.Where(x => x.UserId == user.Id && DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.ToLocalTime().AddHours(3).Date)).ToList();
                //var a = _context.Meals.Where(x => x.UserId == user.Id).ToList();
                //var b = _context.Meals.Where(x => DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.Date)).ToList();
                //var meals2 = _context.Meals.Where(x => DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.Date) && x.UserId == user.Id).ToList();
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<GetMealResp>() { };
                foreach (var meal in meals)
                {
                    var tmeal = new PythModels.PythMeal
                    {
                        description = meal.Description,
                        totalWeight = meal.Weight,
                        type = (mealtype)meal.Type,
                        food = dishes.Where(x => x.MealId == meal.Id).ToList().Select(x => new PythModels.PythFood()
                        {
                            description = x.Description,
                            weight = x.Weight,
                            nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein)
                        }).ToList()
                    };
                    tmeal.pretty = MealHelper.CreatePretty(tmeal.food);
                    resp.Add(new GetMealResp()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime.Value,
                        userId = meal.UserId,
                        meal = tmeal
                    }
                    );
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpGet]
        public ActionResult<List<GetMealResp>> GetUserMeals(int userTgId, int day)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.TgId == userTgId);
                if (user == null)
                    throw new Exception($"I Cant Find User : {userTgId}");
                var inDay = (DayOfWeek)day;
                var startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<GetMealResp>() { };
                foreach (var meal in meals)
                {
                    var tmeal = new PythModels.PythMeal
                    {
                        description = meal.Description,
                        totalWeight = meal.Weight,
                        type = (mealtype)meal.Type,
                        food = dishes.Where(x => x.MealId == meal.Id).ToList().Select(x => new PythModels.PythFood()
                        {
                            description = x.Description,
                            weight = x.Weight,
                            nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein)
                        }).ToList(),
                    };
                    tmeal.pretty = MealHelper.CreatePretty(tmeal.food);
                    resp.Add(new GetMealResp()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime.Value,
                        userId = meal.UserId,
                        meal = tmeal
                    });
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
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
                var inp = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateMealRequest>(request);
                var res = _mealHelper.CreateMeal(inp);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        #endregion
        #endregion


    }
}
