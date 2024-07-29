using System;
using System.Collections.Generic;

namespace NutriDbService.PythModels.Response
{
    public class GetMealResp
    {
        public List<MealResp> Meals { get; set; }

        public string pretty { get; set; }

        public GetMealResp(List<MealResp> meals)
        {
            Meals = meals;
            pretty = string.Empty;
            foreach (var meal in Meals)
            {
                pretty += $"Прием пищи {meal.mealId}";
                var i = 0;
                foreach (var item in meal.meal.food)
                {
                    i++;
                    pretty += $"\n{i}){item.description} {Math.Round(item.weight, 0)}г - {Math.Round(item.nutritional_value.kcal, 0)} ккал ({Math.Round(item.nutritional_value.fats, 0)}г жиров {Math.Round(item.nutritional_value.carbs, 0)}г углеводов {Math.Round(item.nutritional_value.protein, 0)}г белков)";
                }
                pretty += "\n\n";
            }
        }
    }
}
