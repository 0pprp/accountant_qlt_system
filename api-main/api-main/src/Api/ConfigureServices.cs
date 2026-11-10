using System.Text;
using Api.Hubs;
using Api.Services;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Realtime;
using Application.Common.Settings;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class ConfigureServices
{
    public const string DevelopmentCorsPolicy = nameof(DevelopmentCorsPolicy);
    public const string ProductionCorsPolicy = nameof(ProductionCorsPolicy);

    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentCorsPolicy,
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition");
                });

            options.AddPolicy(ProductionCorsPolicy,
                policy =>
                {
                    policy.WithOrigins(
                            "https://panel.qalataldhaman.com",
                            "http://panel.qalataldhaman.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition");
                });
        });

        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddHttpContextAccessor();

        services.AddTransient<ICurrentUserService, CurrentUserService>();

        services.AddSignalR();
        services.AddSingleton<SalesRequestConnectionRegistry>();
        services.AddScoped<ISalesRequestRealtimePublisher, SignalRSalesRequestRealtimePublisher>();
        services.AddScoped<IManagementTaskRealtimePublisher, SignalRManagementTaskRealtimePublisher>();

        services.ConfigureAuthentication(configuration);
    }

    private static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(cfg =>
        {
            BearerTokenSettings bearerTokenSettings = new();
            configuration.GetSection("BearerTokenSettings").Bind(bearerTokenSettings);

            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = bearerTokenSettings.Issuer,
                ValidateIssuer = true,
                ValidAudience = bearerTokenSettings.Audience,
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenSettings.SecretKey)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            cfg.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (string.IsNullOrEmpty(accessToken) == false
                        && (context.HttpContext.Request.Path.StartsWithSegments(SalesRequestHub.Route)
                            || context.HttpContext.Request.Path.StartsWithSegments(ManagementTaskHub.Route)))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<TokenValidator>();


                    var isTokenValid = await tokenValidatorService.ValidateSecurityStamp(context.Principal);

                    if (isTokenValid)
                        context.Success();
                    else
                        context.Fail("Failed to validate security stamp.");
                }
            };
        });

        services.AddAuthorization();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    }
}