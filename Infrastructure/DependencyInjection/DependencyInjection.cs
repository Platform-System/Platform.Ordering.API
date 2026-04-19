using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.Data;
using Platform.Infrastructure.DependencyInjection;
using Platform.Ordering.API.Infrastructure.Data;
using Platform.SystemContext.DependencyInjection;
using StackExchange.Redis;

namespace Platform.Ordering.API.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderingDb");
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379,abortConnect=false";

        services.AddSystemContext();
        services.AddInfrastructure(configuration);

        services.AddDbContext<OrderingDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<OrderingDbContext>());
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        return services;
    }
}
