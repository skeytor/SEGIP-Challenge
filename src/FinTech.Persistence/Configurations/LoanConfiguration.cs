using FinTech.Domain.Entities;
using FinTech.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable(TableNames.Loans);

        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.UserId)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(l => l.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(l => l.Term)
            .IsRequired();

        builder.Property(l => l.InterestRate)
            .IsRequired()
            .HasPrecision(5, 4); //e.g., 0.0500 for 5% interest rate, 0.2400 for 24% interest rate

        builder.Property(l => l.LoanType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.MonthlyPayment)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(l => l.PaymentSchedules)
            .WithOne(ps => ps.Loan)
            .HasForeignKey(ps => ps.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Transactions)
            .WithOne(t => t.Loan)
            .HasForeignKey(t => t.LoanId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
