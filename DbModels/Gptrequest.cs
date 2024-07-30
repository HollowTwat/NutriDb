using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Gptrequest
    {
        public int Id { get; set; }
        public long UserTgid { get; set; }
        public string Answer { get; set; }
        public bool Done { get; set; }
        public bool? Iserror { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? FinishDate { get; set; }
    }
}
