namespace NutriDbService.IntegratorModels
{
    public class FinishPayRequest : BaseRequest
    {
        public override string Status => "A2";
        public decimal PaymentAmount { get; set; }
        public string InvoiceId { get; set; }
        public string SubscriptionType { get; set; }
    }
}
