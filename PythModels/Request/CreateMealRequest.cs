using System;

namespace NutriDbService.PythModels.Request
{
    public class CreateMealRequest
    {
        public int userTgId { get; set; }

        public PythMeal meal { get; set; }

        public string EatedAt { get; set; }
    }
}
