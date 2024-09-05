using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NutriDbService.PythModels
{
    public class PythMeal
    {
        public string description { get; set; }

        public decimal? totalWeight { get; set; }

        public List<PythFood> food { get; set; }

        public mealtype? type { get; set; }

    }
}
