using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Text.Json;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Common.Security.Tokens;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;

public static class SubscriptionsCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds Subscriptions Core services
    /// </summary>
    /// <param name="services">The services collection</param>
    /// <param name="options">The core options</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddSubscriptionsCoreServices(this IServiceCollection services, SubscriptionsCoreServicesOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDbContextPool<SubscriptionDbContext>(dbOptions =>
            dbOptions.UseNpgsql(options.DataConnectionString, (NpgsqlDbContextOptionsBuilder sqlOptions) =>
            {
                sqlOptions.MigrationsAssembly(typeof(SubscriptionDbContext).Assembly.FullName);

                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            })
        );

        services.AddSingleton(options);

        services.AddSingleton(new SubscriptionDataConnectionString(options.DataConnectionString ?? throw new Exception($"{nameof(options)}.{nameof(options.DataConnectionString)} is null")));

        // repositories
        services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();
        services.AddTransient<IBlockedEmailsRepository, BlockedEmailsRepository>();
        services.AddTransient<ITelemetryRepository, TelemetryRepository>();
        services.AddTransient<IRepositories, Repositories>();

        services.AddSingleton<IEmailTemplatesService>(x => new EmailTemplatesService(options.EmailTemplates, options.UriTemplateOptions));

        // configurable dependencies
        services.TryAddSingleton<ICabService>(x => new CabApiService(options.CabApiOptions ?? throw new Exception($"{nameof(options)}.{nameof(options.CabApiOptions)} is null")));
        services.TryAddSingleton<IDateTimeProvider>(x => new RealDateTimeProvider());
        services.TryAddSingleton<IOutboundEmailSender>(x => new OutboundEmailSender(options.GovUkNotifyApiKey ?? throw new Exception($"{nameof(options)}.{nameof(options.GovUkNotifyApiKey)} is null"), options.OutboundEmailSenderMode));

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new EmailAddressConverter());
        services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(options.EncryptionKey ?? throw new Exception($"{nameof(options)}.{nameof(options.EncryptionKey)} is null"), jsonSerializerOptions));

        // the main consumable services
        services.AddTransient<ISubscriptionEngine, SubscriptionEngine>();
        services.AddTransient<ISubscriptionService, SubscriptionService>();

        return services;
    }
}
