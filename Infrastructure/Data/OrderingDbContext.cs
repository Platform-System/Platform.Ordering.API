using Microsoft.EntityFrameworkCore;
using Platform.BuildingBlocks.Abstractions;
using Platform.Infrastructure.Data;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using System.Reflection;

namespace Platform.Ordering.API.Infrastructure.Data;

public sealed class OrderingDbContext : BaseDbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options, ICurrentUserProvider? currentUserProvider = null)
        : base(options, currentUserProvider)
    {
    }

    public DbSet<CartModel> Carts { get; set; }
    public DbSet<CartItemModel> CartItems { get; set; }
    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OrderItemModel> OrderItems { get; set; }
    public DbSet<PaymentModel> Payments { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
