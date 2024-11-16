using System;
using System.Collections.Generic;

namespace NutriDbService.PayModel
{
    public class SuccessPayRequest
    {
        public long TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentCurrency { get; set; }
        public DateTime DateTime { get; set; }
        public string CardId { get; set; }
        public string CardFirstSix { get; set; }
        public string CardLastFour { get; set; }
        public string CardType { get; set; }
        public string CardExpDate { get; set; }
        //public int TestMode { get; set; }
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
        public double IpLatitude { get; set; }
        public double IpLongitude { get; set; }
        public string Issuer { get; set; }
        public string IssuerBankCountry { get; set; }
        public string Description { get; set; }
        public string AuthCode { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public string Token { get; set; }
        public decimal TotalFee { get; set; }
        public string CardProduct { get; set; }
        public string PaymentMethod { get; set; }
        public string FallBackScenarioDeclinedTransactionId { get; set; }
        public string Rrn {  get; set; }
        public List<CustomField> CustomFields { get; set; }
    }
    public class CustomField
    {
        public int Id { get; set; }
    }
}
