namespace NutriDbService.IntegratorModels
{
    public class ProfileAddRequest : BaseRequest
    {
        public override string Status => "A3";
        public string ProfileName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public int WeightKg { get; set; }
        public int HeightCm { get; set; }
        public string Goal { get; set; }
        public int TargetWeightKg { get; set; }
        public int DailyCaloricNormKcal { get; set; }
        public string MacronutrientNormG { get; set; }
        public int WeeklyActivityHours { get; set; }
    }
}
