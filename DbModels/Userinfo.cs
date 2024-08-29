using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Userinfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public short? Age { get; set; }
        public float? Weight { get; set; }
        public float? Height { get; set; }
        public string Gender { get; set; }
        public string Extra { get; set; }
        public string Donelessonlist { get; set; }
        public string MorningPing { get; set; }
        public string EveningPing { get; set; }
        public short? Timeslide { get; set; }
    }
}
