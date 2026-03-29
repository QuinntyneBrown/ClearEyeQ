using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Infrastructure.Channels;
using ClearEyeQ.Notifications.Infrastructure.Consumers;
using ClearEyeQ.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ClearEyeQ.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSql"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(NotificationDbContext).Assembly.FullName));
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string is required.");
            return ConnectionMultiplexer.Connect(connectionString);
        });

        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<IPreferenceRepository, EfNotificationRepository>();

        services.AddHttpClient<FcmPushSender>();
        services.AddHttpClient<SendGridEmailSender>();
        services.AddHttpClient<TwilioSmsSender>();

        services.AddScoped<IChannelSender, FcmPushSender>();
        services.AddScoped<IChannelSender, SignalRInAppSender>();
        services.AddScoped<IChannelSender, SendGridEmailSender>();
        services.AddScoped<IChannelSender, TwilioSmsSender>();

        services.AddScoped<ChannelDispatcher>();

        services.AddScoped<FlareUpWarningConsumer>();
        services.AddScoped<TreatmentReminderConsumer>();
        services.AddScoped<EscalationConsumer>();

        return services;
    }
}
