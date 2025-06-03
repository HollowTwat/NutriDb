using System.Collections.Generic;

namespace NutriDbService.PythModels.Response
{
    public class GetMealTotalResponse
    {

        public decimal TotalKkal { get; set; }

        public decimal GoalKkal { get; set; }

        public decimal TotalProt { get; set; }

        public decimal TotalCarbs { get; set; }

        public decimal TotalFats { get; set; }

        public decimal GoalFats { get; set; }

        public decimal GoalCarbs { get; set; }

        public decimal GoalProt { get; set; }

        public decimal RemainingKK { get; set; }
    }
}
