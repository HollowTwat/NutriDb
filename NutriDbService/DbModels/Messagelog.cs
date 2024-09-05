using System;
using System.Collections.Generic;

namespace NutriDbService.DbModels
{
    public partial class Messagelog
    {
        public int Id { get; set; }
        public string UserMessage { get; set; }
        public string BotMessage { get; set; }
        public int? Step { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
