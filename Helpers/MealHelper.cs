using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.EntityFrameworkCore;
using NutriDbService.DbModels;
using NutriDbService.PythModels;
using NutriDbService.PythModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutriDbService.Helpers
{
    public class MealHelper
    {
        public railwayContext _nutriDbContext { get; set; }
        public MealHelper(railwayContext railwayContext)
        {
            _nutriDbContext = railwayContext;
        }

        public int CreateMeal(CreateMealRequest createMealRequest)
        {
            var user = _nutriDbContext.Users.SingleOrDefault(x => x.TgId == createMealRequest.userTgId);
            if (user == null)
                throw new Exception($"I Cant Find User : {createMealRequest.userTgId}");
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
                    Kkal = null,
                    Weight = d.weight,
                });
                totalweight += d.weight;
            }

            var meal = new Meal()
            {
                UserId = user.Id,
                Weight = createMealRequest.meal.totalWeight == 0 ? totalweight : createMealRequest.meal.totalWeight,
                Dishes = dishes,
                Description = createMealRequest.meal.description,
                Type = (short)createMealRequest.meal.type,
                MealTime = DateTime.UtcNow.ToLocalTime().AddHours(3)//DateTime.TryParseExact(createMealRequest.EatedAt, "dd.MM.yyyy_HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parseTime) == true ? parseTime : null
            };

            _nutriDbContext.Database.BeginTransaction();
            _nutriDbContext.Meals.Add(meal);
            _nutriDbContext.SaveChanges();
            _nutriDbContext.Database.CommitTransaction();
            return meal.Id;
        }
        public int EditMeal(EditMealRequest createMealRequest)
        {
            decimal totalweight = 0;
            var user = _nutriDbContext.Users.SingleOrDefault(x => x.TgId == createMealRequest.userTgId);
            if (user == null)
                throw new Exception($"I Cant Find User : {createMealRequest.userTgId}");

            var meal = _nutriDbContext.Meals.Include(x=>x.Dishes).SingleOrDefault(x => x.Id == createMealRequest.mealId);
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
                    Kkal = null,
                    Weight = d.weight,
                });
                totalweight += d.weight;
            }

            meal.UserId = user.Id;
            meal.Weight = createMealRequest.meal.totalWeight == 0 ? totalweight : createMealRequest.meal.totalWeight;
            meal.Dishes = dishes;
            meal.Description = createMealRequest.meal.description;
            meal.Type = (short)createMealRequest.meal.type;
            if (DateTime.TryParseExact(createMealRequest.EatedAt, "dd.MM.yyyy_HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parseTime))
                meal.MealTime = parseTime;


            _nutriDbContext.Database.BeginTransaction();
            _nutriDbContext.Dishes.RemoveRange(olddishes);
            _nutriDbContext.Meals.Update(meal);
            _nutriDbContext.SaveChanges();
            _nutriDbContext.Database.CommitTransaction();
            return meal.Id;
        }

    }
}
