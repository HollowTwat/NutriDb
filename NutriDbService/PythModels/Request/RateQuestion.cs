namespace NutriDbService.PythModels.Request
{
    public class RateQuestion
    {
        public string food { get; set; }
        public QuesUserInfo user_info { get; set; }
    }

    public class QuesUserInfo
    {
        public short? age { get; set; }
        public string gender { get; set; }
        public decimal? bmi { get; set; }
        public decimal? bmr { get; set; }
        public string allergies { get; set; }
        public string goal { get; set; }
        public decimal? target_calories { get; set; }
    }
}
