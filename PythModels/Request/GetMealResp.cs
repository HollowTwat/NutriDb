using System;

namespace NutriDbService.PythModels.Request
{
    public class GetMealResp
    {
        public int mealId { get; set; }
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime eatedAt { get; set; }
    }

}
