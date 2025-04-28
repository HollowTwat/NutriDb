namespace NutriDbService.IntegratorModels
{
    public class ActiveRequest : BaseRequest
    {
        public override string Status => "A7";
        public string ProfileName { get; set; }
        public string ActivityType { get; set; }
        public string SubscriptionStatus { get; set; }
    }
}
