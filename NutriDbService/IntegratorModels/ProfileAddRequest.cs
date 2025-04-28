namespace NutriDbService.IntegratorModels
{
    public class ProfileAddRequest : BaseRequest
    {
        public override string Status => "A3";
        public string ProfileName { get; set; }
        public string Gender { get; set; }
        public short Age { get; set; }
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
        public string Goal { get; set; }
        public decimal TargetWeightKg { get; set; }
        public decimal DailyCaloricNormKcal { get; set; }
        public string MacronutrientNormG { get; set; }
        public double WeeklyActivityHours { get; set; }
    }
}
