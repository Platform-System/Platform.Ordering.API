using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Platform.Catalog.Grpc;
using Platform.Infrastructure.Data;
using Platform.Infrastructure.DependencyInjection;
using Platform.Ordering.API.Infrastructure.Constants;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Infrastructure.Data;
using Platform.Ordering.API.Infrastructure.Integrations.Catalog;
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
        services.Configure<CatalogClientOptions>(configuration.GetSection(ConfigurationSections.CatalogIntegration));
        services.AddGrpcClient<CatalogIntegration.CatalogIntegrationClient>((sp, options) =>
        {
            var catalogOptions = sp.GetRequiredService<IOptions<CatalogClientOptions>>().Value;
            options.Address = new Uri(
                string.IsNullOrWhiteSpace(catalogOptions.Address)
                    ? "http://localhost"
                    : catalogOptions.Address);
        });
        services.AddScoped<ICatalogClient, CatalogClient>();

        return services;
    }
}
