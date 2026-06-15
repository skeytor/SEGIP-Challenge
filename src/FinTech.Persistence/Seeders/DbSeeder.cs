using FinTech.Domain.Entities;
using FinTech.Domain.Utils;
using SharedKernel.Utils;

namespace FinTech.Persistence.Seeders;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Loans.Any())
        {
            return; // DB has been seeded
        }

        List<Loan> loans =
        [
            BuildLoan(
                amount: 5_000,
                termMonths: 12,
                status: LoanStatus.Active,
                createdAt: new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)),

            BuildLoan(
                amount: 15_000,
                termMonths: 24,
                status: LoanStatus.Pending,
                createdAt: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
        ];

        context.Loans.AddRange(loans);
        context.SaveChanges();
    }

    private static Loan BuildLoan(decimal amount, int termMonths, LoanStatus status, DateTime createdAt)
    {
        decimal tem = FinancialCalculator.GetMonthlyRate(FinancialConstants.AnnualInterestRate);
        decimal monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(amount, tem, termMonths);
        DateTime firstPaymentDate = createdAt.AddMonths(1);

        List<PaymentInstallment> installments = FinancialCalculator.GenerateFixedSchedule(
            amount,
            FinancialConstants.AnnualInterestRate,
            termMonths,
            firstPaymentDate);

        return new Loan
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid().ToString(),
            Amount = amount,
            Term = termMonths,
            InterestRate = FinancialConstants.AnnualInterestRate,
            LoanType = LoanType.Fixed,
            Status = status,
            MonthlyPayment = monthlyPayment,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            PaymentSchedules = [.. installments.Select(s => new PaymentSchedule
            {
                Id = Guid.NewGuid(),
                PaymentNumber = s.PaymentNumber,
                DueDate = s.DueDate,
                TotalPayment = s.TotalPayment,
                Principal = s.Principal,
                Interest = s.Interest,
                RemainingBalance = s.RemainingBalance,
                Status = PaymentScheduleStatus.Pending,
            })]
        };
    }
}
