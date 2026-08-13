using Domain.Common;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.ExpenseAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.ActivityLogAggregate;
using Domain.Entities.NotificationAggregate;
using Domain.Entities.PurchaseAggregate;
using Domain.Entities.SafeAggregate;
using Domain.Entities.WarehouseAggregate;

namespace Domain.Entities.BranchAggregate;

public class Branch : AuditableEntity
{
    public Branch(string name, int provinceId)
    {
        Name = name;
        ProvinceId = provinceId;
        Safe = new Safe(name);
        Warehouses = new HashSet<Warehouse>();
        OrderLists = new HashSet<OrderList>();
        UserBranches = new HashSet<UserBranch>();
        Customers = new HashSet<Customer>();
        Orders = new HashSet<Order>();
        Expenses = new HashSet<Expense>();
        Purchases = new HashSet<Purchase>();
        Notifications = new HashSet<Notification>();
        ActivityLogs = new HashSet<ActivityLog>();
    }

    public string Name { get; set; }
    public int ProvinceId { get; set; }

    public Province? Province { get; set; }
    public Safe? Safe { get; set; }
    public ICollection<Warehouse> Warehouses { get; set; }
    public ICollection<OrderList> OrderLists { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; }
    public ICollection<Customer> Customers { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Expense> Expenses { get; set; }
    public ICollection<Purchase> Purchases { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    public ICollection<ActivityLog> ActivityLogs { get; set; }

    public void Update(string name, int provinceId)
    {
        Name = name;
        ProvinceId = provinceId;
    }
}