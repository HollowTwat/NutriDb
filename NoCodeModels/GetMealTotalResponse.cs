using System.Collections.Generic;

namespace NutriDbService.NoCodeModels
{
    public class GetMealTotalResponse
    {

        public decimal TotalKkal { get; set; }

        public decimal GoalKkal { get; set; }

        public decimal TotalProt { get; set; }

        public decimal TotalCarbs { get; set; }

        public decimal TotalFats { get; set; }

    }
}
