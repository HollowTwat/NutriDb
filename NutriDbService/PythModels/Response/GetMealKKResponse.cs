using System;
using System.Collections.Generic;

namespace NutriDbService.PythModels.Response
{

    public class GetMealKKResponse
    {
        public static DateTime GetFirstDayOfWeek(DateTime date)
        {
            DayOfWeek firstDay = DayOfWeek.Monday;
            int diff = (7 + (date.DayOfWeek - firstDay)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        public GetMealKKResponse(Periods period)
        {
            days = new List<DaysMeal>();
            var now = DateTime.UtcNow.ToLocalTime().AddHours(3).Date;
            var startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
            switch (period)
            {
                case Periods.day:
                    startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-1).Date;
                    break;
                case Periods.week:
                    startDate = GetFirstDayOfWeek(now);
                    break;
                case Periods.mathweek:
                    startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-7).Date;
                    break;
                case Periods.math3weeks:
                    startDate = DateTime.UtcNow.ToLocalTime().AddHours(3).AddDays(-21).Date;
                    break;
                case Periods.month:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }
            for (DateTime kdate = startDate; kdate <= now; kdate = kdate.AddDays(1))
            {
                days.Add(new DaysMeal
                {
                    date = kdate.Date,
                    isEmpty = true,
                    Meals = null,
                    totalKK = 0m
                });
            }
        }
        public Dictionary<string, string> user_info { get; set; }
        public decimal total_avg_period { get; set; }
        public List<DaysMeal> days { get; set; }
    }
    public class DaysMeal
    {

        public DateTime date { get; set; }
        public bool isEmpty { get; set; }
        public decimal totalKK { get; set; }
        public List<PythMeal> Meals { get; set; }
    }
}
