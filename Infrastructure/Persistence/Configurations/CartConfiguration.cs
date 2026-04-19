using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<CartModel>
{
    public void Configure(EntityTypeBuilder<CartModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
