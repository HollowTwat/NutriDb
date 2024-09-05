using System;

namespace NutriDbService.PythModels.Response
{
    public class MealResponse
    {
        public int mealId { get; set; }
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime eatedAt { get; set; }

    }

}
