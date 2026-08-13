using Application.Common.Interfaces;
using Application.Common.Settings;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure;

public static class ConfigureServices
{
    public static void AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<AppSaveChangeInterceptor>();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder
            .EnableDynamicJson()
            .Build();
        services.AddSingleton(dataSource);

        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(ConfigureDbContextOptions);

        services.AddDbContextFactory<ApplicationDbContext>(
            ConfigureDbContextOptions,
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
    
    private static void ConfigureDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>());

        options.AddInterceptors(serviceProvider.GetRequiredService<AppSaveChangeInterceptor>());
    }
}