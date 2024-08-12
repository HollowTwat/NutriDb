using System.Collections.Generic;

namespace NutriDbService.NoCodeModels
{
    public class GetWeekMealStatusResponse
    {
        public string DisplayDay { get; set; }

        public string isEmpty { get; set; }

        public List<MealStatus> MealStatus { get; set; }
    }
    public class MealStatus
    {
        public string isEmpty { get; set; }

        public mealtype Type { get; set; }
    }
}
