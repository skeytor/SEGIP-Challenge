namespace FinTech.Domain.Utils;

/// <summary>
/// Provides utility methods for financial calculations, including converting annual effective rates to monthly effective rates,
/// calculating fixed monthly payments, and generating payment schedules.
/// </summary>
public static class FinancialCalculator
{
    /// <summary>
    /// Converts the "Tasa Efectiva Anual" (TAE) to the "Tasa Efectiva Mensual" (TEM) using the formula:
    /// TEM = (1 + TAE)^(1/12) - 1
    /// </summary>
    /// <param name="annualRate">The annual effective rate (TAE)</param>
    /// <returns>The monthly effective rate (TEM)</returns>
    public static decimal GetMonthlyRate(decimal annualRate)
        => (decimal)(Math.Pow((double)(1 + annualRate), 1.0 / 12) - 1);

    /// <summary>
    /// Calculates the fixed monthly payment for a loan using the French system formula:
    /// <b>Payment = Amount * [TEM * (1 + TEM)^n] / [(1 + TEM)^n - 1]</b>
    /// </summary>
    /// <param name="amount">The loan amount</param>
    /// <param name="monthlyRate">The monthly effective rate (TEM)</param>
    /// <param name="termMonths">The loan term in months</param>
    /// <returns> The fixed monthly payment </returns>
    public static decimal CalculateFixedMonthlyPayment(decimal amount, decimal monthlyRate, int termMonths)
    {
        // Edge case: If the monthly rate is 0, the payment is simply the amount divided by the total months
        if (monthlyRate == 0)
        {
            return Math.Round(amount / termMonths, 2);
        }

        double r = (double)monthlyRate; // TEM
        double n = termMonths;
        double factor = Math.Pow(1 + r, n); // (1 + TEM)^n

        // Payment = Amount * [TEM * (1 + TEM)^n] / [(1 + TEM)^n - 1]
        decimal payment = (decimal)(((double)amount * r * factor) / (factor - 1));
        return Math.Round(payment, 2);
    }

    /// <summary>
    /// Generates a fixed payment schedule for a loan based on the French system. 
    /// Each installment includes the payment number, due date, total payment, principal, interest, and remaining balance.
    /// </summary>
    /// <param name="amount">The loan amount</param>
    /// <param name="annualRate">The annual effective rate (TAE)</param>
    /// <param name="termMonths">The loan term in months</param>
    /// <param name="firstPaymentDate">The date of the first payment</param>
    /// <returns>A list of payment installments</returns>
    public static List<PaymentInstallment> GenerateFixedSchedule(
        decimal amount,
        decimal annualRate,
        int termMonths,
        DateTime firstPaymentDate)
    {
        // Calculate the monthly effective rate (TEM) from the annual effective rate (TAE)
        decimal monthlyRate = GetMonthlyRate(annualRate);

        // Calculate the fixed monthly payment using the French system formula
        decimal monthlyPayment = CalculateFixedMonthlyPayment(amount, monthlyRate, termMonths);

        List<PaymentInstallment> schedule = [];

        // Initialize the remaining balance to the original loan amount
        decimal remainingBalance = amount;

        for (int i = 1; i <= termMonths; i++)
        {
            // Calculate interest for the current month
            decimal interest = Math.Round(remainingBalance * monthlyRate, 2);
            decimal principal = Math.Round(monthlyPayment - interest, 2);

            remainingBalance = Math.Round(remainingBalance - principal, 2);

            schedule.Add(new PaymentInstallment(
                PaymentNumber: i,
                DueDate: firstPaymentDate.AddMonths(i - 1),
                TotalPayment: monthlyPayment,
                Principal: principal,
                Interest: interest,
                RemainingBalance: remainingBalance
            ));
        }
        return schedule;
    }
}

/// <summary>
/// Represents a single payment installment in a loan payment schedule
/// </summary>
/// <param name="PaymentNumber">The number of the payment installment</param>
/// <param name="DueDate">The date when the payment is due</param>
/// <param name="TotalPayment">The total amount of the payment</param>
/// <param name="Principal">The portion of the payment that reduces the loan balance</param>
/// <param name="Interest">The interest component of the payment</param>
/// <param name="RemainingBalance">The remaining loan balance after the payment</param>
public sealed record PaymentInstallment(
    int PaymentNumber,
    DateTime DueDate,
    decimal TotalPayment,
    decimal Principal,
    decimal Interest,
    decimal RemainingBalance);
