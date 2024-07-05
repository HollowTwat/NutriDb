using System;

namespace NutriDbService.PythModels.Request
{
    public class GetMealResp
    {
        public int mealId { get; set; }
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime eatedAt { get; set; }
        public string CreatePretty()
        {
            var tpretty = string.Empty;
            var i = 0;
            foreach (var item in meal.food)
            {
                i++;
                tpretty += $"\n{i}){item.description} {item.weight}г ({item.nutritional_value.fats}г жиров {item.nutritional_value.carbs}г углеводов {item.nutritional_value.protein}г белков) ";
            }
            return tpretty;
        }
    }

}
