using System;

namespace NutriDbService.PythModels.Request
{
    public class GetMealResp
    {
        public int userId { get; set; }

        public PythMeal meal { get; set; }

        public DateTime EatedAt { get; set; }
    }

}
