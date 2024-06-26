using System.Collections.Generic;

namespace NutriDbService.PythModels
{
    public class PythMeal
    {
        public string description { get; set; }

        public decimal? totalWeight { get; set; }

        public List<PythFood> food { get; set; }

        public mealtype type { get; set; }
    }
    public class PythMeal2
    {
        public string description { get; set; }

        public decimal? totalWeight { get; set; }

        public List<string> food { get; set; }

        public mealtype type { get; set; }
    }
}
