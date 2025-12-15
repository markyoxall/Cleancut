using CleanCut.Application.Common.Interfaces;
using CleanCut.Infrastructure.Shared.Messaging;
using CleanCut.Infrastructure.Shared.Email;
// order export registrations are in the BackgroundServices project
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CleanCut.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Map RabbitMQ options manually (avoid extra package dependency)
        var rabbitSection = configuration.GetSection("RabbitMQ");
        var rabbitOptions = new RabbitMqOptions
        {
            Hostname = rabbitSection["Hostname"] ?? "localhost",
            Port = int.TryParse(rabbitSection["Port"], out var p) ? p : 5672,
            VirtualHost = rabbitSection["VirtualHost"] ?? "/",
            Username = rabbitSection["Username"] ?? "guest",
            Password = rabbitSection["Password"] ?? "guest",
            Exchange = rabbitSection["Exchange"] ?? "cleancut.events",
            OrderCreatedRoutingKey = rabbitSection["OrderCreatedRoutingKey"] ?? "order.created"
        };
        services.AddSingleton(rabbitOptions);

        // Map SMTP options
        var smtpSection = configuration.GetSection("Smtp");
        var smtpOptions = new SmtpOptions
        {
            Host = smtpSection["Host"] ?? "localhost",
            Port = int.TryParse(smtpSection["Port"], out var sp) ? sp : 1025,
            UseSsl = bool.TryParse(smtpSection["UseSsl"], out var ss) ? ss : false,
            Username = smtpSection["Username"] ?? string.Empty,
            Password = smtpSection["Password"] ?? string.Empty,
            FromAddress = smtpSection["FromAddress"] ?? "no-reply@cleancut.local",
            FromName = smtpSection["FromName"] ?? "CleanCut"
        };
        services.AddSingleton(smtpOptions);

        // Order export is registered by the background services project

        // Messaging services
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<IRabbitMqRetryQueue, RabbitMqRetryQueue>();
        // Integration event publisher (generic)
        services.AddSingleton<CleanCut.Application.Common.Interfaces.IIntegrationEventPublisher, IntegrationEventPublisher>();
        // Note: exchange initialization removed to keep startup simple; create exchange via management UI or a separate initializer if needed

        // Email sender
        services.AddSingleton<IEmailSender>(sp => new SmtpEmailSender(sp.GetRequiredService<SmtpOptions>(), sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SmtpEmailSender>>()));

        return services;
    }
}
