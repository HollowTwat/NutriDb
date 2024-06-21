using System;
using System.Collections.Generic;

namespace NutriDbService.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; }

    public short? Timezone { get; set; }

    public DateOnly RegistrationTime { get; set; }

    public int? StageId { get; set; }

    public int? LessonId { get; set; }

    public bool IsActive { get; set; }

    public int TgId { get; set; }

    public virtual ICollection<Loyalty> Loyalties { get; set; } = new List<Loyalty>();

    public virtual ICollection<Messagelog> Messagelogs { get; set; } = new List<Messagelog>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<Userinfo> Userinfos { get; set; } = new List<Userinfo>();
}
