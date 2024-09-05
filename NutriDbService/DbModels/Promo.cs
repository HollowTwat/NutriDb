using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Promo
    {
        public Promo()
        {
            Subscriptions = new HashSet<Subscription>();
        }

        public int Id { get; set; }
        public string PromoCode { get; set; }
        public short? Discount { get; set; }
        public short? Freeperiod { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
