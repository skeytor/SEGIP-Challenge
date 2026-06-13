namespace FinTech.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public Guid? LoanId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public virtual Loan? Loan { get; set; }
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
}

public enum TransactionType
{
    Disbursement,
    Payment,
    Transfer
}

