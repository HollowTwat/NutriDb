using System;
using System.Collections.Generic;

namespace Nutri2Service.Models;

public partial class Promo
{
    public int Id { get; set; }

    public string PromoCode { get; set; }

    public short? Discount { get; set; }

    public short? Freeperiod { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
