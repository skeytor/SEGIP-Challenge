namespace FinTech.Domain.Entities;

public class Loan : BaseEntity<Guid>
{
    public string UserId { get; set; } = null!;
    public decimal Amount { get; set; }
    public int Term { get; set; } // Months
    public decimal InterestRate { get; set; } // TEA (Annual Effective Rate)
    public LoanType LoanType { get; set; }
    public LoanStatus Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public virtual ICollection<PaymentSchedule> PaymentSchedules { get; set; } = [];
    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}

public enum LoanStatus
{
    Pending,
    Approved,
    Rejected,
    Active
}

public enum LoanType
{
    Fixed,
    Decreasing
}
