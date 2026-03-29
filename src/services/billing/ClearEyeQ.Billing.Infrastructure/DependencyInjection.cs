using ClearEyeQ.Billing.Application.Interfaces;
using ClearEyeQ.Billing.Infrastructure.Payments;
using ClearEyeQ.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClearEyeQ.Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSql"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(BillingDbContext).Assembly.FullName));
        });

        services.AddScoped<ISubscriptionRepository, EfSubscriptionRepository>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<StripeWebhookHandler>();

        return services;
    }
}
