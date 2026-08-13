namespace Application.Common.SeedData;

public static class RolePermissions
{
    public static readonly List<RolePermissionDto> All = [];
    
    static RolePermissions()
    {
        var admin = new RolePermissionDto
        {
            RoleName = Roles.Admin.Name,
            Permissions = Permissions.All.Select(x => x.Name).ToList()
        };
        All.Add(admin);

        var chiefAccountant = new RolePermissionDto
        {
            RoleName = Roles.ChiefAccountant.Name,
            Permissions = Permissions.All.Select(x => x.Name).ToList()
        };
        All.Add(chiefAccountant);

        var mainAccountant = new RolePermissionDto
        {
            RoleName = Roles.MainAccountant.Name,
            Permissions = Permissions.All.Select(x => x.Name).ToList()
        };
        All.Add(mainAccountant);

        var branchAccountant = new RolePermissionDto
        {
            RoleName = Roles.BranchAccountant.Name
        };
        branchAccountant.Permissions.Add(Permissions.InstallmentPayment.Read);
        branchAccountant.Permissions.Add(Permissions.Order.Read);
        branchAccountant.Permissions.Add(Permissions.User.Read);
        branchAccountant.Permissions.Add(Permissions.Customer.Read);
        branchAccountant.Permissions.Add(Permissions.Warehouse.Read);
        branchAccountant.Permissions.Add(Permissions.ProductCategory.Read);
        branchAccountant.Permissions.Add(Permissions.Product.Read);
        branchAccountant.Permissions.Add(Permissions.Purchase.Read);
        branchAccountant.Permissions.Add(Permissions.Expense.Read);
        branchAccountant.Permissions.Add(Permissions.Safe.Read);
        branchAccountant.Permissions.Add(Permissions.Transaction.Read);
        branchAccountant.Permissions.Add(Permissions.Transaction.CollectCash);
        branchAccountant.Permissions.Add(Permissions.Report.Read);
        branchAccountant.Permissions.Add(Permissions.Notification.Read);
        branchAccountant.Permissions.Add(Permissions.Notification.Update);
        branchAccountant.Permissions.Add(Permissions.ActivityLog.Read);
        All.Add(branchAccountant);

        var branchManager = new RolePermissionDto
        {
            RoleName = Roles.BranchManager.Name
        };
        branchManager.Permissions.Add(Permissions.InstallmentPayment.Read);
        branchManager.Permissions.Add(Permissions.Order.Read);
        branchManager.Permissions.Add(Permissions.User.Read);
        branchManager.Permissions.Add(Permissions.Customer.Read);
        branchManager.Permissions.Add(Permissions.Warehouse.Read);
        branchManager.Permissions.Add(Permissions.ProductCategory.Read);
        branchManager.Permissions.Add(Permissions.Product.Read);
        branchManager.Permissions.Add(Permissions.Purchase.Read);
        branchManager.Permissions.Add(Permissions.Expense.Read);
        branchManager.Permissions.Add(Permissions.Safe.Read);
        branchManager.Permissions.Add(Permissions.Transaction.Read);
        branchManager.Permissions.Add(Permissions.Report.Read);
        branchManager.Permissions.Add(Permissions.Notification.Read);
        branchManager.Permissions.Add(Permissions.Notification.Update);
        branchManager.Permissions.Add(Permissions.ActivityLog.Read);
        All.Add(branchManager);

        var ceo = new RolePermissionDto
        {
            RoleName = Roles.Ceo.Name
        };
        ceo.Permissions.Add(Permissions.InstallmentPayment.Read);
        ceo.Permissions.Add(Permissions.Order.Read);
        ceo.Permissions.Add(Permissions.User.Read);
        ceo.Permissions.Add(Permissions.Customer.Read);
        ceo.Permissions.Add(Permissions.Warehouse.Read);
        ceo.Permissions.Add(Permissions.ProductCategory.Read);
        ceo.Permissions.Add(Permissions.Product.Read);
        ceo.Permissions.Add(Permissions.Purchase.Read);
        ceo.Permissions.Add(Permissions.Expense.Read);
        ceo.Permissions.Add(Permissions.Safe.Read);
        ceo.Permissions.Add(Permissions.Transaction.Read);
        ceo.Permissions.Add(Permissions.Report.Read);
        ceo.Permissions.Add(Permissions.Notification.Read);
        ceo.Permissions.Add(Permissions.Notification.Update);
        ceo.Permissions.Add(Permissions.ActivityLog.Read);
        All.Add(ceo);

        var mandob = new RolePermissionDto
        {
            RoleName = Roles.Mandob.Name
        };
        mandob.Permissions.Add(Permissions.InstallmentPayment.Create);
        mandob.Permissions.Add(Permissions.InstallmentPayment.Read);
        mandob.Permissions.Add(Permissions.Order.Read);
        mandob.Permissions.Add(Permissions.User.Read);
        mandob.Permissions.Add(Permissions.Customer.Read);
        mandob.Permissions.Add(Permissions.Warehouse.Read);
        mandob.Permissions.Add(Permissions.OrderList.Read);
        mandob.Permissions.Add(Permissions.Purchase.Read);
        mandob.Permissions.Add(Permissions.Expense.Read);
        mandob.Permissions.Add(Permissions.Safe.Read);
        All.Add(mandob);
        
        var motaba = new RolePermissionDto
        {
            RoleName = Roles.Motaba.Name
        };
        motaba.Permissions.Add(Permissions.InstallmentPayment.Create);
        motaba.Permissions.Add(Permissions.InstallmentPayment.Read);
        motaba.Permissions.Add(Permissions.Order.Read);
        motaba.Permissions.Add(Permissions.User.Read);
        motaba.Permissions.Add(Permissions.Customer.Read);
        motaba.Permissions.Add(Permissions.Warehouse.Read);
        motaba.Permissions.Add(Permissions.OrderList.Read);
        motaba.Permissions.Add(Permissions.Purchase.Read);
        motaba.Permissions.Add(Permissions.Expense.Read);
        motaba.Permissions.Add(Permissions.Safe.Read);
        All.Add(motaba);
    }
}

public record RolePermissionDto
{
    public required string RoleName { get; set; }
    public List<string> Permissions { get; set; } = [];
}