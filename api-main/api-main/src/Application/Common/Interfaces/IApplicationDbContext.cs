using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.NotificationAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProductAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.WarehouseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext : IAsyncDisposable
{
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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    public DbSet<TEntity> Set<TEntity>() where TEntity : class;
}