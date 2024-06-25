using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Meal
    {
        public Meal()
        {
            Dishes = new HashSet<Dish>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public decimal? Weight { get; set; }
        public DateTime? Timestamp { get; set; }
        public short? Type { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Dish> Dishes { get; set; }
    }
}
