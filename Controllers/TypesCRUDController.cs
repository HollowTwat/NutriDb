using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.Helpers;
using NutriDbService.PythModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult<int> CreateMeal(CreateMealRequest request)
        {
            try
            {
                var res = _mealHelper.CreateMeal(request);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        [HttpGet]
        public ActionResult<List<GetMealResp>> GetTodayUserMeals(int userId)
        {
            try
            {
                var meals = _context.Meals.Where(x => x.UserId == userId && x.Timestamp.Value.Date == DateTime.Now.Date).ToList();
                var mealsId = meals.Select(x => x.Id).ToList();
                var dishes = _context.Dishes.Where(x => mealsId.Contains(x.MealId));
                var resp = new List<GetMealResp>() { };
                foreach (var meal in meals)
                {
                    resp.Add(new GetMealResp()
                    {
                        EatedAt = meal.Timestamp.Value,
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
                                nutriProps = new PythModels.NutriProps(x.Fats, x.Carbs, x.Protein)
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
        #endregion
        #endregion


    }
}
