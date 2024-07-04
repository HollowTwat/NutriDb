using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;

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
                var user = _context.Users.Single(x => x.TgId == userTgId);
                var meals = _context.Meals.Where(x => x.UserId == user.Id && DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.Date)).ToList();
                //var a = _context.Meals.Where(x => x.UserId == user.Id).ToList();
                //var b = _context.Meals.Where(x => DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.Date)).ToList();
                //var meals2 = _context.Meals.Where(x => DateTime.Equals(x.MealTime.Value.Date, DateTime.UtcNow.Date) && x.UserId == user.Id).ToList();
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<GetMealResp>() { };
                foreach (var meal in meals)
                {
                    resp.Add(new GetMealResp()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime.Value,
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
                                nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein)
                            }).ToList(),
                        }
                    });
                }
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpGet]
        public ActionResult<List<GetMealResp>> GetUserMeals(int userTgId, string day)
        {
            try
            {
                var user = _context.Users.Single(x => x.TgId == userTgId);
                var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date == DateTime.Now.Date).ToList();
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<GetMealResp>() { };
                foreach (var meal in meals)
                {
                    resp.Add(new GetMealResp()
                    {
                        mealId = meal.Id,
                        eatedAt = meal.MealTime.Value,
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
                                nutritional_value = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein)
                            }).ToList(),
                        }
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
