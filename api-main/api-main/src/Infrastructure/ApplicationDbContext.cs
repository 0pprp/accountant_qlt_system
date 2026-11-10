using System.Reflection;
using Application.Common.Interfaces;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.FollowerWorkAggregate;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.DelegateFollowupAggregate;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.NotificationAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProductAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SalesLocationAggregate;
using Domain.Entities.SalesRequestAggregate;
using SalesRequestTimelineEntry = Domain.Entities.SalesRequestAggregate.SalesRequestTimelineEntry;
using Domain.Entities.SalesFollowupAggregate;
using Domain.Entities.SalesShiftAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.WarehouseAggregate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IWebHostEnvironment _environment;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IWebHostEnvironment environment) : base(options)
    {
        _environment = environment;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_environment.IsProduction() == false)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderList> OrderLists { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<UserPermissionDenial> UserPermissionDenials { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserBranch> UserBranches { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseItem> ExpenseItems { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<Safe> Safes { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<SafeTransferTransaction> SafeTransferTransactions { get; set; }
    public DbSet<SellerCashDeliveryTransaction> SellerCashDeliveryTransactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<SalesShift> SalesShifts { get; set; }
    public DbSet<SalesLocationPoint> SalesLocationPoints { get; set; }
    public DbSet<Domain.Entities.SalesEmployeeAggregate.SalesEmployeeAssignment> SalesEmployeeAssignments { get; set; }
    public DbSet<SalesRequest> SalesRequests { get; set; }
    public DbSet<SalesRequestTimelineEntry> SalesRequestTimelineEntries { get; set; }
    public DbSet<SalesRequestChangeOutbox> SalesRequestChangeOutboxes { get; set; }
    public DbSet<SalesEmployeeAppBlock> SalesEmployeeAppBlocks { get; set; }
    public DbSet<DirectorSalesEmployeeBoardRating> DirectorSalesEmployeeBoardRatings { get; set; }
    public DbSet<DirectorBoardOperatorListRating> DirectorBoardOperatorListRatings { get; set; }
    public DbSet<ListSupervisionDayRating> ListSupervisionDayRatings { get; set; }
    public DbSet<FollowerDelegateDayRating> FollowerDelegateDayRatings { get; set; }
    public DbSet<SalesEmployeeListDayRating> SalesEmployeeListDayRatings { get; set; }
    public DbSet<ManagementTask> ManagementTasks { get; set; }
    public DbSet<ManagementTaskEvent> ManagementTaskEvents { get; set; }
    public DbSet<ManagementTaskAttachment> ManagementTaskAttachments { get; set; }
    public DbSet<ManagementTaskChangeOutbox> ManagementTaskChangeOutboxes { get; set; }
    public DbSet<FollowerSaleAuthentication> FollowerSaleAuthentications { get; set; }
    public DbSet<FollowerCustomerRatingSnapshot> FollowerCustomerRatingSnapshots { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<FollowerFieldFollowup> FollowerFieldFollowups { get; set; }
    public DbSet<FollowerDelegateNote> FollowerDelegateNotes { get; set; }
    public DbSet<LegalFollowupCase> LegalFollowupCases { get; set; }
    public DbSet<LegalFollowupCaseDocument> LegalFollowupCaseDocuments { get; set; }
    public DbSet<Domain.Entities.CommitteeTaskAggregate.CommitteeTaskItem> CommitteeTaskItems { get; set; }
    public DbSet<Domain.Entities.AppNoteAggregate.AppNote> AppNotes { get; set; }
    public DbSet<Domain.Entities.WaiverAggregate.CustomerWaiverRequest> CustomerWaiverRequests { get; set; }
    public DbSet<Domain.Entities.CustomerDecisionAggregate.CustomerDecision> CustomerDecisions { get; set; }
    public DbSet<Domain.Entities.LegalConversionAggregate.LegalConversionRequest> LegalConversionRequests { get; set; }
    public DbSet<Domain.Entities.PriceBoardAggregate.PriceBoardEntry> PriceBoardEntries { get; set; }
    public DbSet<Domain.Entities.AnnouncementAggregate.Announcement> Announcements { get; set; }
    public DbSet<Domain.Entities.MobileAppReleaseAggregate.MobileAppRelease> MobileAppReleases { get; set; }
    public DbSet<Domain.Entities.TutorialVideoAggregate.TutorialVideo> TutorialVideos { get; set; }
    public DbSet<Domain.Entities.WrongSaleAggregate.WrongSaleNotice> WrongSaleNotices { get; set; }
    public DbSet<Domain.Entities.ComplaintAggregate.ComplaintMessage> ComplaintMessages { get; set; }
    public DbSet<Domain.Entities.BranchVisitAggregate.BranchVisit> BranchVisits { get; set; }
    public DbSet<Domain.Entities.BranchVisitAggregate.BranchVisitRating> BranchVisitRatings { get; set; }
}