using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Common.Utilities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Infrastructure;

public static class ConfigureServices
{
    public static void AddInfrastructureService(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? hostEnvironment = null)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<AppSaveChangeInterceptor>();
        services.AddScoped<ISalesRequestSubmissionLock, SalesRequestSubmissionLock>();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder
            .EnableDynamicJson()
            .Build();
        services.AddSingleton(dataSource);

        var ignorePendingModelChanges = PendingModelChangesGate.ShouldIgnorePendingModelChanges(
            configuration,
            hostEnvironment?.EnvironmentName ?? configuration["ASPNETCORE_ENVIRONMENT"],
            connectionString);

        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>((sp, options) =>
            ConfigureDbContextOptions(sp, options, ignorePendingModelChanges));

        services.AddDbContextFactory<ApplicationDbContext>(
            (sp, options) => ConfigureDbContextOptions(sp, options, ignorePendingModelChanges),
            ServiceLifetime.Scoped);

        services.AddScoped<IApplicationDbContextFactory, ApplicationDbContextFactory>();

        services.AddScoped<DatabaseInitializer>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>();
        services.AddScoped<TokenValidator>();
        services.AddSingleton<TokenFactoryService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<IFileManager, FileManager>();

        services.AddHostedService<SalesRequestOutboxDispatcher>();
        services.AddHostedService<ManagementTaskOutboxDispatcher>();

        services.AddMemoryCache();

        services.AddSingleton(_ =>
        {
            BearerTokenSettings bearerTokenSettings = new();
            configuration.GetRequiredSection("BearerTokenSettings").Bind(bearerTokenSettings);

            ArgumentException.ThrowIfNullOrEmpty(bearerTokenSettings.SecretKey, nameof(bearerTokenSettings.SecretKey));

            return bearerTokenSettings;
        });
        
        services.AddSingleton(_ =>
        {
            AdminData adminData = new();
            configuration.GetRequiredSection("AdminData").Bind(adminData);

            return adminData;
        });
    }
    
    private static void ConfigureDbContextOptions(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder options,
        bool ignorePendingModelChanges)
    {
        options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>());

        if (ignorePendingModelChanges)
        {
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        options.AddInterceptors(serviceProvider.GetRequiredService<AppSaveChangeInterceptor>());
    }
}
