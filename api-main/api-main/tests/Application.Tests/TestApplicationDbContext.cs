using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.FollowerWorkAggregate;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.DelegateFollowupAggregate;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.NotificationAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProductAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SalesFollowupAggregate;
using Domain.Entities.SalesLocationAggregate;
using Domain.Entities.SalesRequestAggregate;
using SalesRequestTimelineEntry = Domain.Entities.SalesRequestAggregate.SalesRequestTimelineEntry;
using Domain.Entities.SalesShiftAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.WarehouseAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Ignore(x => x.Location);
            entity.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId);
            entity.HasMany(x => x.InstallmentPayments)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId);
            entity.HasOne(x => x.Seller)
                .WithMany(x => x.SoldOrders)
                .HasForeignKey(x => x.SellerId);
            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.BranchId);
            entity.HasOne(x => x.OrderList)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.OrderListId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ComplexProperty(x => x.Business);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Ignore(x => x.SalaryDetail);
            entity.Ignore(x => x.OrderListAsMandob);
            entity.Ignore(x => x.OrderListsAsMotaba);
        });

        modelBuilder.Entity<Safe>(entity =>
        {
            entity.HasOne(x => x.Branch)
                .WithOne(x => x.Safe)
                .HasForeignKey<Safe>(x => x.BranchId);
            entity.HasMany(x => x.Transactions)
                .WithOne(x => x.Safe)
                .HasForeignKey(x => x.SafeId);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasMany(x => x.PurchaseItems)
                .WithOne(x => x.Purchase)
                .HasForeignKey(x => x.PurchaseId);
            entity.HasMany(x => x.Transactions)
                .WithOne(x => x.Purchase)
                .HasForeignKey(x => x.PurchaseId);
        });

        modelBuilder.Entity<Product>()
            .HasMany(x => x.OrderItems)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<OrderList>(entity =>
        {
            entity.HasOne(x => x.Mandob)
                .WithMany()
                .HasForeignKey(x => x.MandobId);
            entity.HasOne(x => x.Motaba)
                .WithMany()
                .HasForeignKey(x => x.MotabaId);
            entity.HasOne(x => x.Branch)
                .WithMany(x => x.OrderLists)
                .HasForeignKey(x => x.BranchId);
        });

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);

        modelBuilder.Entity<SellerCashDeliveryTransaction>(entity =>
        {
            entity.HasOne(x => x.Seller)
                .WithMany(x => x.SellerCashDeliveryTransactions)
                .HasForeignKey(x => x.SellerId);
            entity.HasOne(x => x.Transaction)
                .WithOne(x => x.SellerCashDeliveryTransaction)
                .HasForeignKey<SellerCashDeliveryTransaction>(x => x.TransactionId);
        });

        modelBuilder.Entity<SafeTransferTransaction>(entity =>
        {
            entity.HasOne(x => x.SourceSafe)
                .WithMany()
                .HasForeignKey(x => x.SourceSafeId);
            entity.HasOne(x => x.DestinationSafe)
                .WithMany()
                .HasForeignKey(x => x.DestinationSafeId);
        });

        modelBuilder.Entity<UserBranch>().HasKey(x => new { x.UserId, x.BranchId });
        modelBuilder.Entity<UserPermission>().HasKey(x => new { x.UserId, x.PermissionId });
        modelBuilder.Entity<UserPermissionDenial>().HasKey(x => new { x.UserId, x.PermissionId });
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<User>().Property(x => x.PermissionsVersion).IsConcurrencyToken();

        base.OnModelCreating(modelBuilder);
    }
}
