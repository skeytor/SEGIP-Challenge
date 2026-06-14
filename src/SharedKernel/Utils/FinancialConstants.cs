namespace SharedKernel.Utils;

public static class FinancialConstants
{
    public const decimal AnnualInterestRate = 0.24m; // 24% annual effective rate (TEA)
    public const int MaxActiveLoansPerUser = 3;
    public const decimal MaxIncomeRatio = 0.40m; // Maximum 40% of monthly income for loan payments
    public const int MinTermMonths = 6;
    public const int MaxTermMonths = 60;
    public const decimal MinLoanAmount = 500;
    public const decimal MaxLoanAmount = 50_000;
}
