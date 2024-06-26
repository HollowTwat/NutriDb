using System;

namespace NutriDbService.PythModels.Request
{
    public class CreateMealRequest
    {
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime EatedAt { get; set; }
    }
}
