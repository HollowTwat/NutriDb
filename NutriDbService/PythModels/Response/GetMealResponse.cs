using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NutriDbService.PythModels.Response
{
    public class GetMealResponseNoPretty
    {
        public List<MealResponse> Meals { get; set; }
    }
    public class GetMealResponse: GetMealResponseNoPretty
    {
       // public List<MealResponse> Meals { get; set; }

        public string pretty { get; set; }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        public GetMealResponse(List<MealResponse> meals)
        {
            Meals = meals;
            pretty = string.Empty;
            foreach (var meal in Meals)
            {
                pretty += $"Прием пищи {meal.mealId}";

                pretty += $"\n{GetEnumDescription((mealtype)meal.meal.type)}";
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
