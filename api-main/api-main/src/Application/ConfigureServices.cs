using System.Reflection;
using Application.Common.Interfaces;
using Application.Common.MediatorBehaviours;
using Application.Common.Services;
using Application.Common.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Application;

public static class ConfigureServices
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        var webHostEnvironment = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
        
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = true;
        QuestPDF.Settings.FontDiscoveryPaths.Clear();
        QuestPDF.Settings.FontDiscoveryPaths.Add(Path.Combine(webHostEnvironment!.WebRootPath, "fonts"));
        services.AddSingleton<PdfReportGenerator>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
    }
}