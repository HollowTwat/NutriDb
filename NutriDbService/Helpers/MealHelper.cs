using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NutriDbService.DbModels;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using NutriDbService.PythModels.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NutriDbService.Helpers
{
    public class MealHelper
    {
        public railwayContext _nutriDbContext { get; set; }
        private readonly ILogger _logger;
        public MealHelper(railwayContext railwayContext, IServiceProvider serviceProvider)
        {
            _nutriDbContext = railwayContext;
            _logger = serviceProvider.GetRequiredService<ILogger<MealHelper>>();
        }

        public async Task<int> CreateMeal(EditMealRequest createMealRequest)
        {
            var user = await _nutriDbContext.Users.SingleOrDefaultAsync(x => x.TgId == createMealRequest.userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {createMealRequest.userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }
            var dishes = new HashSet<Dish>();
            decimal totalweight = 0;
            foreach (var d in createMealRequest.meal.food)
            {
                dishes.Add(new Dish
                {
                    Carbs = d.nutritional_value.carbs,
                    Fats = d.nutritional_value.fats,
                    Protein = d.nutritional_value.protein,
                    Description = d.description,
                    Kkal = d.nutritional_value.kcal,
                    Weight = d.weight,
                });
                totalweight += d.weight;
            }

            Meal meal;
            if (createMealRequest.mealId == null)
            {
                var IsTyme = DateTime.TryParseExact(createMealRequest.EatedAt, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parseTime);

                meal = new Meal()
                {
                    UserId = user.Id,
                    Weight = createMealRequest.meal.totalWeight == 0 ? totalweight : createMealRequest.meal.totalWeight,
                    Dishes = dishes,
                    Description = createMealRequest.meal.description,
                    Type = (short)createMealRequest.meal.type,

                    MealTime = IsTyme ? parseTime : DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide))
                };

                var oldmeal = await _nutriDbContext.Meals.SingleOrDefaultAsync(x => x.UserId == meal.UserId && x.MealTime.Date == meal.MealTime.Date && x.Type == meal.Type);
                if (oldmeal != null)
                {
                    await _nutriDbContext.Database.BeginTransactionAsync();
                    if (meal.Type != 5)
                        _nutriDbContext.Dishes.RemoveRange(_nutriDbContext.Dishes.Where(x => x.MealId == oldmeal.Id));
                    foreach (var di in dishes)
                        di.MealId = oldmeal.Id;
                    await _nutriDbContext.Dishes.AddRangeAsync(dishes);
                    await _nutriDbContext.SaveChangesAsync();
                    await _nutriDbContext.Database.CommitTransactionAsync();
                    return oldmeal.Id;
                }
                else
                {
                    await _nutriDbContext.Database.BeginTransactionAsync();
                    await _nutriDbContext.Meals.AddAsync(meal);
                    await _nutriDbContext.SaveChangesAsync();
                    await _nutriDbContext.Database.CommitTransactionAsync();

                    return meal.Id;
                }
            }
            else
            {
                meal = await _nutriDbContext.Meals.SingleOrDefaultAsync(x => x.Id == createMealRequest.mealId);
                foreach (var dish in dishes)
                    dish.MealId = (int)createMealRequest.mealId;
                meal.Weight += createMealRequest.meal.totalWeight == 0 ? totalweight : createMealRequest.meal.totalWeight;

                await _nutriDbContext.Database.BeginTransactionAsync();
                _nutriDbContext.Meals.Update(meal);
                await _nutriDbContext.Dishes.AddRangeAsync(dishes);
                await _nutriDbContext.SaveChangesAsync();
                await _nutriDbContext.Database.CommitTransactionAsync();
                return meal.Id;
            }
        }
        public async Task<int> DeleteMeal(int mealId, long userTgId)
        {
            var user = await _nutriDbContext.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }

            Meal meal;

            var oldmeal = await _nutriDbContext.Meals.SingleOrDefaultAsync(x => x.UserId == user.Id && x.Id == mealId);
            if (oldmeal != null)
            {
                await _nutriDbContext.Database.BeginTransactionAsync();
                _nutriDbContext.Dishes.RemoveRange(_nutriDbContext.Dishes.Where(x => x.MealId == oldmeal.Id));
                _nutriDbContext.Meals.Remove(oldmeal);
                await _nutriDbContext.SaveChangesAsync();
                await _nutriDbContext.Database.CommitTransactionAsync();
                return oldmeal.Id;
            }
            else
            {
                await ErrorHelper.SendErrorMess($"Не найден Meal {mealId}");
                return 0;
            }

        }

        public async Task<int> EditMeal(EditMealRequest createMealRequest)
        {
            decimal totalweight = 0;
            var user = await _nutriDbContext.Users.SingleOrDefaultAsync(x => x.TgId == createMealRequest.userTgId);
            if (user == null)
                throw new Exception($"I Cant Find User : {createMealRequest.userTgId}");

            var meal = await _nutriDbContext.Meals.Include(x => x.Dishes).SingleOrDefaultAsync(x => x.Id == createMealRequest.mealId);
            if (meal == null)
                throw new Exception($"I Cant Find Meal : {createMealRequest.mealId}");
            var olddishes = meal.Dishes;
            var dishes = new HashSet<Dish>();
            foreach (var d in createMealRequest.meal.food)
            {
                dishes.Add(new Dish
                {
                    Carbs = d.nutritional_value.carbs,
                    Fats = d.nutritional_value.fats,
                    Protein = d.nutritional_value.protein,
                    Description = d.description,
                    Kkal = d.nutritional_value.kcal,
                    Weight = d.weight,
                });
                totalweight += d.weight;
            }

            meal.UserId = user.Id;
            meal.Weight = createMealRequest.meal.totalWeight == 0 ? totalweight : createMealRequest.meal.totalWeight;
            meal.Dishes = dishes;
            meal.Description = createMealRequest.meal.description;
            meal.Type = (short)createMealRequest.meal.type;
            // Не нужно, так как редактируется старый прием пищи
            //if (DateTime.TryParseExact(createMealRequest.EatedAt, "dd.MM.yyyy_HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parseTime))
            //    meal.MealTime = parseTime;


            await _nutriDbContext.Database.BeginTransactionAsync();
            _nutriDbContext.Dishes.RemoveRange(olddishes);
            _nutriDbContext.Meals.Update(meal);
            await _nutriDbContext.SaveChangesAsync();
            await _nutriDbContext.Database.CommitTransactionAsync();
            return meal.Id;
        }

        public async Task<List<MealResponse>> GetMeals(GetUserMealsRequest req)
        {
            var user = await _nutriDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == req.userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {req.userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }
            //var inDay = (DayOfWeek)day;
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
            var startDate = now.AddDays(-7).Date;
            switch (req.period)
            {
                case Periods.day:
                    startDate = now.AddDays(-1).Date;
                    break;
                case Periods.week:
                    startDate = GetFirstDayOfWeek(now);
                    break;
                case Periods.mathweek:
                    startDate = now.AddDays(-7).Date;
                    break;
                case Periods.math3weeks:
                    startDate = now.AddDays(-21).Date;
                    break;
                case Periods.month:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }
            //var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
            var meals = await _nutriDbContext.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
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
            var dishes = await _nutriDbContext.Dishes.AsNoTracking().Where(x => mealsId.Contains(x.MealId)).ToListAsync();
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
                        }).ToList(),
                    }
                });
            }
            return resp;
        }

        public async Task<List<MealResponse>> GetMealsForAnal(long userTgId)
        {
            var user = await _nutriDbContext.Users.SingleOrDefaultAsync(x => x.TgId == userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }
            var resp = new List<MealResponse>() { };
            //var inDay = (DayOfWeek)day;
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
            var startDate = now.AddDays(-7).Date;
            //var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
            var meals = await _nutriDbContext.Meals.Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
            if (meals.Count < 5)
                meals = await _nutriDbContext.Meals.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).Take(5).ToListAsync();
            if (!meals.Any())
                return resp;
            var mealsId = meals.Select(x => x.Id).ToList();
            var dishes = await _nutriDbContext.Dishes.Where(x => mealsId.Contains(x.MealId)).ToListAsync();

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
                        }).ToList(),
                    }
                });
            }
            return resp;
        }

        public async Task<List<KKMealResponse>> GetMealsKK(GetUserMealsRequest req)
        {
            var user = await _nutriDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == req.userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {req.userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }
            //var inDay = (DayOfWeek)day;
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
            var startDate = now.AddDays(-7).Date;
            switch (req.period)
            {
                case Periods.day:
                    startDate = now.AddDays(-1).Date;
                    break;
                case Periods.week:
                    startDate = GetFirstDayOfWeek(now);
                    break;
                case Periods.mathweek:
                    startDate = now.AddDays(-7).Date;
                    break;
                case Periods.math3weeks:
                    startDate = now.AddDays(-21).Date;
                    break;
                case Periods.month:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }
            //var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
            var meals = await _nutriDbContext.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
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
            var dishes = await _nutriDbContext.Dishes.AsNoTracking().Where(x => mealsId.Contains(x.MealId)).ToListAsync();
            var resp = new List<KKMealResponse>() { };
            foreach (var meal in meals)
            {

                resp.Add(new KKMealResponse()
                {
                    date = meal.MealTime,
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
            return resp;
        }

        public async Task<MealResponse> GetSingleMeal(GetUserMealsRequest req)
        {
            var user = await _nutriDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == req.userTgId);
            if (user == null)
            {
                var mes = $"I Cant Find User : {req.userTgId}";
                await ErrorHelper.SendSystemMess(mes);
                throw new Exception(mes);
            }
            //var inDay = (DayOfWeek)day;
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
            var startDate = now.AddDays(-7).Date;

            var meals = await _nutriDbContext.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();
            if (!String.IsNullOrEmpty(req.dayStr))
            {
                meals = meals.Where(x => x.MealTime.Date == DateTime.ParseExact(req.dayStr, "dd.MM.yyyy", CultureInfo.InvariantCulture).Date).ToList();
            }
            else
            {
                var mes = $"Не пришел dayStr: {Newtonsoft.Json.JsonConvert.SerializeObject(req)}";
                await ErrorHelper.SendSystemMess(mes);
                throw new ArgumentNullException(mes);
            }
            if (req.typemeal != null)
            {
                meals = meals.Where(x => x.Type == ((short)req.typemeal)).ToList();
            }
            else
            {
                var mes = $"Не пришел typemeal: {Newtonsoft.Json.JsonConvert.SerializeObject(req)}";
                await ErrorHelper.SendSystemMess(mes);
                throw new ArgumentNullException(mes);
            }

            var meal = meals.SingleOrDefault();
            if (meal == null)
                return new MealResponse();
            var dishes = await _nutriDbContext.Dishes.AsNoTracking().Where(x => x.MealId == meal.Id).ToListAsync();
            var resp = new MealResponse()
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
            };
            return resp;
        }

        public async Task<List<MealResponse>> GetMealInMathMonthByDate(GetUserMealsRequest req)
        {
            var user = await _nutriDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.TgId == req.userTgId);
            if (user == null)
                throw new Exception($"I Cant Find User : {req.userTgId}");
            //var inDay = (DayOfWeek)day;
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).AddHours(Decimal.ToDouble(user.Timeslide)).Date;
            var startDate = now.AddMonths(-1).Date;

            //var meals = _context.Meals.Where(x => x.UserId == user.Id && x.MealTime.Value.Date > startDate && x.MealTime.Value.DayOfWeek == (DayOfWeek)day).ToList();
            var meals = await _nutriDbContext.Meals.AsNoTracking().Where(x => x.UserId == user.Id && x.MealTime.Date > startDate).ToListAsync();

            if (!String.IsNullOrEmpty(req.dayStr))
            {
                meals = meals.Where(x => x.MealTime.Date == DateTime.ParseExact($"{req.dayStr}.{startDate.Year}", "dd.MM.yyyy", CultureInfo.InvariantCulture).Date).ToList();
            }
            if (req.typemeal != null)
            {
                meals = meals.Where(x => x.Type == ((short)req.typemeal)).ToList();
            }
            var mealsId = meals.Select(x => x.Id).ToList();
            var dishes = await _nutriDbContext.Dishes.AsNoTracking().Where(x => mealsId.Contains(x.MealId)).ToListAsync();
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
                        }).ToList(),
                    }
                });
            }
            return resp;
        }

        public static DateTime GetFirstDayOfWeek(DateTime date)
        {
            DayOfWeek firstDay = DayOfWeek.Monday;
            int diff = (7 + (date.DayOfWeek - firstDay)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}
