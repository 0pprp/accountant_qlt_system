using System.Reflection;
using Application.Common.Interfaces;
using Application.Common.MediatorBehaviours;
using Application.Common.Services;
using Application.Common.Utilities;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Realtime;
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
        services.AddScoped<ISalesRequestChangeNotifier, SalesRequestChangeNotifier>();
        services.AddScoped<SalesRequestHubIdentityResolver>();
        services.AddScoped<SalesRequestOutboxProcessor>();
        services.AddScoped<ManagementTaskOutboxProcessor>();
        services.AddScoped<Application.SalesFollowup.Common.SalesFollowupDataService>();
        services.AddScoped<Application.SalesOverview.Common.SalesOverviewDataService>();
        services.AddScoped<Application.SalesEmployees.Common.SalesEmployeesDataService>();
        services.AddScoped<Application.SalesEmployeeTracking.Common.SalesEmployeeTrackingDataService>();
        services.AddScoped<Application.SalesRecords.Common.SalesRecordsDataService>();
        services.AddScoped<Application.LegalFollowups.Common.LegalFollowupDataService>();
        services.AddScoped<Application.DelegateFollowup.Common.DelegateFollowupDataService>();
        services.AddScoped<Application.ManagementSummary.Common.ManagementSummaryDataService>();
        services.AddScoped<Application.ManagementRatings.Common.ManagementRatingsDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerWorkDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerSalesReviewDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerDecliningDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerNotesDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerOldCustomersDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerDelegateNotesDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerCollectionDataService>();
        services.AddScoped<Application.FollowerWork.Common.FollowerTrackingDataService>();
        services.AddScoped<Application.Committee.Queries.App.GetListStats.CommitteeListStatsBuilder>();
        services.AddScoped<Application.InstallmentPayments.Common.AppInstallmentPaymentWriter>();
    }
}