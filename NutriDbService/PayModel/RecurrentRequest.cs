namespace NutriDbService.PayModel
{
    public class RecurrentRequest
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool RequireConfirmation { get; set; }
        public string StartDate { get; set; }
        public string Interval { get; set; }
        public int Period { get; set; }
        public string Status { get; set; }
        public int SuccessfulTransactionsNumber { get; set; }
        public int FailedTransactionsNumber { get; set; }
        public int MaxPeriods { get; set; }
        public string LastTransactionDate { get; set; }
        public string NextTransactionDate { get; set; }
    }
}
