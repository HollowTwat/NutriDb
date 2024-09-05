using System;

namespace NutriDbService.PythModels.Request
{
    public class CreateMealRequest
    {
        public long userTgId { get; set; }

        public PythMeal meal { get; set; }

        public string EatedAt { get; set; }
    }
}
