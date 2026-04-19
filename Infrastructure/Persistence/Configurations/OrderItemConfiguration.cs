using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemModel>
{
    public void Configure(EntityTypeBuilder<OrderItemModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.HasIndex(x => new { x.OrderId, x.ProductId, x.Type });

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId);
    }
}
