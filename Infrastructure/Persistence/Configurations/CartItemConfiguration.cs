using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItemModel>
{
    public void Configure(EntityTypeBuilder<CartItemModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CartId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.HasIndex(x => new { x.CartId, x.ProductId });

        builder.HasOne(x => x.Cart)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CartId);
    }
}
