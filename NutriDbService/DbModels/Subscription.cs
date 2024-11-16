using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? PromoId { get; set; }
        public long? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DateTime { get; set; }
        public string Status { get; set; }
        public string InvoiceId { get; set; }
        public string AccountId { get; set; }
        public string SubscriptionId { get; set; }
        public string Email { get; set; }
        public string Rrn { get; set; }
        public string Extra { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; }

        public virtual Promo Promo { get; set; }
        public virtual User User { get; set; }
    }
}
