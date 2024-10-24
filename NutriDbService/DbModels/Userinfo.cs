using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Userinfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public short? Age { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string Gender { get; set; }
        public string Extra { get; set; }
        public string Donelessonlist { get; set; }
        public string MorningPing { get; set; }
        public string EveningPing { get; set; }
        public decimal? Timeslide { get; set; }
        public decimal? Goalkk { get; set; }
        public string Goal { get; set; }
        public DateTime? LastlessonTime { get; set; }
    }
}
