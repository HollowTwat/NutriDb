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
        public string CardId { get; set; }
        public string CardFirstSix { get; set; }

        public string CardLastFour { get; set; }

        public string CardType { get; set; }

        public string CardExpDate { get; set; }

        public bool TestMode { get; set; }

        public string Status { get; set; }

        public string OperationType { get; set; }
        public string GatewayName { get; set; }
        public string InvoiceId { get; set; }
        public string AccountId { get; set; }
        public string SubscriptionId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
        public string IpAddress { get; set; }
        public string IpCountry { get; set; }
        public string IpCity { get; set; }
        public string IpRegion { get; set; }

        public string IpDistrict { get; set; }

        public string IpLatitude { get; set; }

        public string IpLongitude { get; set; }
        public string Issuer { get; set; }
        public string IssuerBankCountry { get; set; }
        public string Description { get; set; }
        public string AuthCode { get; set; }
        public string Data { get; set; }
        public string Token { get; set; }
        public decimal TotalFee { get; set; }
        public string CardProduct { get; set; }
        public string PaymentMethod { get; set; }
        public long FallBackScenarioDeclinedTransactionId { get; set; }
        public string Rrn {  get; set; }
        public string CustomFields { get; set; }

    }
}
