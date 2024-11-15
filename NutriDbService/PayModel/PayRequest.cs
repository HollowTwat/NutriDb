using System;

namespace NutriDbService.PayModel
{
    public class PayRequest
    {
        public long TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentAmount { get; set; }
        public string PaymentCurrency { get; set; }
        public string DateTime { get; set; }
        public string CardFirstSix { get; set; }
        public string CardLastFour { get; set; }
        public string CardType { get; set; }
        public string CardExpDate { get; set; }
        public bool TestMode { get; set; }
        public string Status { get; set; }
        public string OperationType { get; set; }
        public string GatewayName { get; set; }
        public string CustomFields { get; set; }
    }
}
