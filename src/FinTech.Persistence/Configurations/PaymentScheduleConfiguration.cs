using FinTech.Domain.Entities;
using FinTech.Persistence.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Persistence.Configurations;

internal sealed class PaymentScheduleConfiguration : IEntityTypeConfiguration<PaymentSchedule>
{
    public void Configure(EntityTypeBuilder<PaymentSchedule> builder)
    {
        builder.ToTable(TableNames.PaymentSchedules);

        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.PaymentNumber)
            .IsRequired();

        builder.Property(ps => ps.DueDate)
            .IsRequired();

        builder.Property(ps => ps.TotalPayment)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ps => ps.Interest)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ps => ps.RemainingBalance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ps => ps.Status)
            .IsRequired()
            .HasConversion<string>();

        // Unique constraint to ensure one payment schedule per loan and payment number
        builder.HasIndex(ps => new { ps.LoanId, ps.PaymentNumber });
    }
}
