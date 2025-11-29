using CleanCut.Application.Common.Interfaces;
using CleanCut.Infrastructure.Shared.Messaging;
using CleanCut.Infrastructure.Shared.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CleanCut.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Map RabbitMQ options manually
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
            Port = int.TryParse(smtpSection["Port"], out var sp) ? sp : 25,
            UseSsl = bool.TryParse(smtpSection["UseSsl"], out var ss) ? ss : false,
            Username = smtpSection["Username"] ?? string.Empty,
            Password = smtpSection["Password"] ?? string.Empty,
            FromAddress = smtpSection["FromAddress"] ?? "no-reply@cleancut.local",
            FromName = smtpSection["FromName"] ?? "CleanCut"
        };
        services.AddSingleton(smtpOptions);

        // Messaging services
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<IRabbitMqRetryQueue, RabbitMqRetryQueue>();

        // Email sender
        services.AddSingleton<IEmailSender>(sp => new SmtpEmailSender(sp.GetRequiredService<SmtpOptions>(), sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SmtpEmailSender>>()));

        return services;
    }
}
