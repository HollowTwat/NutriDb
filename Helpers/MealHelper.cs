using NutriDbService.DbModels;
using NutriDbService.PythModels.Request;
using System.Collections.Generic;

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
            }
            var meal = new Meal()
            {
                UserId = createMealRequest.userId,
                Weight = createMealRequest.meal.totalWeight,
                Dishes = dishes,
                Description = createMealRequest.meal.description,
                Type = (short)createMealRequest.meal.type,
                Timestamp = createMealRequest.EatedAt
            };
            _nutriDbContext.Database.BeginTransaction();
            _nutriDbContext.Meals.Add(meal);
            _nutriDbContext.SaveChanges();
            _nutriDbContext.Database.CommitTransaction();
            return meal.Id;
        }
    }
}
