namespace NutriDbService.IntegratorModels
{
    public class PayRequest : BaseRequest
    {
        public override string Status => "A1";
        public decimal PaymentAmount { get; set; }
        public string SubscriptionType { get; set; }
    }
}
