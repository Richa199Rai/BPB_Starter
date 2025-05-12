namespace BankingApi.Models
{
    public class Account
    {
        public string AccountId { get; set; }
        public string? DisplayName { get; set; }
        public string? AccountType { get; set; }
        public string? AccountStatus { get; set; }
        public string? Currency { get; set; }
        public string? OpeningDate { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    
}