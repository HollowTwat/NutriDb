using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Dish
    {
        public int Id { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbs { get; set; }
        public decimal Protein { get; set; }
        public decimal Kkal { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public int MealId { get; set; }

        public virtual Meal Meal { get; set; }
    }
}
