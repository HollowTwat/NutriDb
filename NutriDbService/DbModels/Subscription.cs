using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly? SubscriptionStartDate { get; set; }
        public DateOnly? SubscriptionEndDate { get; set; }
        public string PaymentMethod { get; set; }
        public int? PromoId { get; set; }

        public virtual Promo Promo { get; set; }
        public virtual User User { get; set; }
    }
}
