using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class User
    {
        public User()
        {
            Loyalties = new HashSet<Loyalty>();
            Meals = new HashSet<Meal>();
            Messagelogs = new HashSet<Messagelog>();
            Subscriptions = new HashSet<Subscription>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public short? Timezone { get; set; }
        public DateOnly RegistrationTime { get; set; }
        public int? StageId { get; set; }
        public int? LessonId { get; set; }
        public bool IsActive { get; set; }
        public long TgId { get; set; }
        public long? UserNoId { get; set; }

        public virtual ICollection<Loyalty> Loyalties { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
        public virtual ICollection<Messagelog> Messagelogs { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
