using NutriDbService.DbModels;
using NutriDbService.PythModels.Request;
using System.Collections.Generic;

namespace NutriDbService.Helpers
{
    public class MealHelper
    {
        public NutriDbContext _nutriDbContext { get; set; }
        public MealHelper(NutriDbContext railwayContext)
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
                    Carbs = d.nutriProps.carbs,
                    Fats = d.nutriProps.fats,
                    Protein = d.nutriProps.protein,
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
