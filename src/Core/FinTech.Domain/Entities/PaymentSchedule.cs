namespace FinTech.Domain.Entities;

public class PaymentSchedule : BaseEntity<Guid>
{
    public Guid LoanId { get; set; }
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentScheduleStatus Status { get; set; }
    public virtual Loan Loan { get; set; } = null!;
}

public enum PaymentScheduleStatus
{
    Pending,
    Paid
}
