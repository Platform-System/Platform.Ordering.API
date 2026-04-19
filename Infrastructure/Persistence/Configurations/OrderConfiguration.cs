using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderModel>
{
    public void Configure(EntityTypeBuilder<OrderModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.OrderCode)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .IsRequired();

        builder.Property(x => x.ExpiredAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.OrderCode)
            .IsUnique();
    }
}
