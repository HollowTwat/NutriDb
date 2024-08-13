using System.Collections.Generic;

namespace NutriDbService.NoCodeModels
{
    public class GetWeekMealStatusResponse
    {
        public string DisplayDay { get; set; }

        public bool isEmpty { get; set; }

        public decimal TotalKkal {  get; set; } 
        public List<MealStatus> MealStatus { get; set; }
    }
    public class MealStatus
    {
        public bool isEmpty { get; set; }

        public mealtype Type { get; set; }
    }
}
