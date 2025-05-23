﻿using System;
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
            Userinfos = new HashSet<Userinfo>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public DateOnly RegistrationTime { get; set; }
        public int? StageId { get; set; }
        public int? LessonId { get; set; }
        public bool IsActive { get; set; }
        public long TgId { get; set; }
        public bool? NotifyStatus { get; set; }
        public string Email { get; set; }
        public decimal Timeslide { get; set; }

        public virtual ICollection<Loyalty> Loyalties { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
        public virtual ICollection<Messagelog> Messagelogs { get; set; }
        public virtual ICollection<Userinfo> Userinfos { get; set; }
    }
}
