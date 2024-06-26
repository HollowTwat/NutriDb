using System;

namespace NutriDbService.PythModels.Request
{
    public class CreateMealRequest
    {
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime EatedAt { get; set; }
    }
    public class CreateMealRequest2
    {
        public int userId { get; set; }

        public PythMeal2 meal { get; set; }

        public DateTime EatedAt { get; set; }
    }
}
