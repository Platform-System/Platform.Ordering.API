using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentModel>
{
    public void Configure(EntityTypeBuilder<PaymentModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.PaymentLinkId)
            .HasMaxLength(100);

        builder.Property(x => x.CheckoutUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Amount)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.PaymentLinkId)
            .IsUnique()
            .HasFilter("\"PaymentLinkId\" IS NOT NULL");

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId);
    }
}
