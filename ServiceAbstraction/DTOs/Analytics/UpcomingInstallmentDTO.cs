namespace ServiceAbstraction.DTOs.Analytics
{
    public class UpcomingInstallmentDTO
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset NextPaymentDate { get; set; }
        public int RemainingPayments { get; set; }
    }
}
