namespace Application.Common.SeedData;

public static class RolePermissions
{
    public static readonly List<RolePermissionDto> All = [];
    
    static RolePermissions()
    {
        var admin = new RolePermissionDto
        {
            RoleName = Roles.Admin.Name,
            Permissions = Permissions.All.Select(x => x.Name)
                .Append(Permissions.User.ManagePermissions)
                .Concat(Permissions.SalesRequest.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesFollowup.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesOverview.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesEmployee.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesLocation.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesRoute.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.SalesSummary.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.ManagementTask.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.DelegateFollowup.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.ManagementSummary.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.ManagementRatings.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.FollowerWork.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.LegalFollowup.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.CommitteeTask.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.AppNote.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.Waiver.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.CustomerDecision.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.LegalNames.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.LegalConversion.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.PriceBoard.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.Announcement.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.Tutorial.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.MobileAppRelease.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.WrongSale.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.BranchNewSale.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.BranchDeclining.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.Complaint.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.BranchVisit.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.BranchManagerAccount.OutsideAll.Select(x => x.Name))
                .Concat(Permissions.DelegateHome.OutsideAll.Select(x => x.Name))
                .ToList()
        };
        All.Add(admin);

        var chiefAccountant = new RolePermissionDto
        {
            RoleName = Roles.ChiefAccountant.Name,
            Permissions = Permissions.All.Select(x => x.Name)
                .Append(Permissions.Waiver.Read)
                .Append(Permissions.Waiver.Apply)
                .Append(Permissions.CustomerDecision.Read)
                .Append(Permissions.LegalConversion.Decide)
                .ToList()
        };
        All.Add(chiefAccountant);

        var mainAccountant = new RolePermissionDto
        {
            RoleName = Roles.MainAccountant.Name,
            Permissions = Permissions.All.Select(x => x.Name)
                .Append(Permissions.Waiver.Read)
                .Append(Permissions.Waiver.Apply)
                .Append(Permissions.CustomerDecision.Read)
                .Append(Permissions.PriceBoard.Read)
                .ToList()
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
        branchAccountant.Permissions.Add(Permissions.Waiver.Read);
        branchAccountant.Permissions.Add(Permissions.Waiver.Apply);
        branchAccountant.Permissions.Add(Permissions.PriceBoard.Read);
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
        branchManager.Permissions.Add(Permissions.AppNote.Create);
        branchManager.Permissions.Add(Permissions.Waiver.Create);
        branchManager.Permissions.Add(Permissions.Waiver.Read);
        branchManager.Permissions.Add(Permissions.CustomerDecision.Create);
        branchManager.Permissions.Add(Permissions.CustomerDecision.Read);
        branchManager.Permissions.Add(Permissions.LegalNames.Read);
        branchManager.Permissions.Add(Permissions.OrderList.Read);
        branchManager.Permissions.Add(Permissions.WrongSale.Read);
        branchManager.Permissions.Add(Permissions.WrongSale.MarkBranchReviewed);
        branchManager.Permissions.Add(Permissions.BranchNewSale.Read);
        branchManager.Permissions.Add(Permissions.BranchNewSale.MarkRead);
        branchManager.Permissions.Add(Permissions.BranchDeclining.Read);
        branchManager.Permissions.Add(Permissions.BranchVisit.Read);
        branchManager.Permissions.Add(Permissions.BranchVisit.Open);
        branchManager.Permissions.Add(Permissions.BranchVisit.Rate);
        branchManager.Permissions.Add(Permissions.BranchVisit.Depart);
        branchManager.Permissions.Add(Permissions.Complaint.Submit);
        branchManager.Permissions.Add(Permissions.ManagementRatings.Read);
        branchManager.Permissions.Add(Permissions.ManagementRatings.DetailsRead);
        // مهامي (inbox) — قراءة + إجراءات المكلّف؛ بلا Create/Assign للنظام
        branchManager.Permissions.Add(Permissions.ManagementTask.Read);
        branchManager.Permissions.Add(Permissions.ManagementTask.Accept);
        branchManager.Permissions.Add(Permissions.ManagementTask.Reject);
        branchManager.Permissions.Add(Permissions.ManagementTask.Complete);
        branchManager.Permissions.Add(Permissions.ManagementTask.Confirm);
        branchManager.Permissions.Add(Permissions.ManagementTask.ReadAttachment);
        // لا User.ManagePermissions — حسابات الفرع عبر BranchManagerAccount فقط
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
        mandob.Permissions.Add(Permissions.Order.Create);
        mandob.Permissions.Add(Permissions.Order.Read);
        mandob.Permissions.Add(Permissions.Order.Update);
        mandob.Permissions.Add(Permissions.Product.Read);
        mandob.Permissions.Add(Permissions.Customer.Create);
        mandob.Permissions.Add(Permissions.Customer.Read);
        mandob.Permissions.Add(Permissions.Attachment.Create);
        mandob.Permissions.Add(Permissions.Attachment.Update);
        mandob.Permissions.Add(Permissions.User.Read);
        mandob.Permissions.Add(Permissions.Warehouse.Read);
        mandob.Permissions.Add(Permissions.OrderList.Read);
        mandob.Permissions.Add(Permissions.Purchase.Read);
        mandob.Permissions.Add(Permissions.Expense.Read);
        mandob.Permissions.Add(Permissions.Safe.Read);
        // صلاحيات App لطلبات البيع — تفويض صريح خارج Permissions.All
        mandob.Permissions.Add(Permissions.SalesRequest.AppRead);
        mandob.Permissions.Add(Permissions.SalesRequest.AppView);
        mandob.Permissions.Add(Permissions.SalesRequest.AppPrepare);
        mandob.Permissions.Add(Permissions.SalesRequest.AppPend);
        mandob.Permissions.Add(Permissions.SalesRequest.AppReject);
        mandob.Permissions.Add(Permissions.SalesRequest.AppInspect);
        mandob.Permissions.Add(Permissions.SalesRequest.AppLinkDraft);
        mandob.Permissions.Add(Permissions.SalesRequest.AppTransferName);
        mandob.Permissions.Add(Permissions.SalesRequest.SubmitEmployee);
        mandob.Permissions.Add(Permissions.SalesRequest.SubmitDelegate);
        mandob.Permissions.Add(Permissions.AppNote.Create);
        // شكوى موظف المبيعات — مسار Complaints المنفصل (ليس AppNote).
        mandob.Permissions.Add(Permissions.Complaint.Submit);
        mandob.Permissions.Add(Permissions.Announcement.AppRead);
        mandob.Permissions.Add(Permissions.Tutorial.AppRead);
        mandob.Permissions.Add(Permissions.PriceBoard.Read);
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
        motaba.Permissions.Add(Permissions.SalesRequest.SubmitFollower);
        motaba.Permissions.Add(Permissions.FollowerWork.SalesReviewAuthenticate);
        motaba.Permissions.Add(Permissions.FollowerWork.NotesWrite);
        motaba.Permissions.Add(Permissions.FollowerWork.DelegateNotesWrite);
        motaba.Permissions.Add(Permissions.LegalFollowup.FieldRead);
        motaba.Permissions.Add(Permissions.LegalFollowup.FieldUpload);
        motaba.Permissions.Add(Permissions.LegalFollowup.FieldComplete);
        motaba.Permissions.Add(Permissions.CommitteeTask.AppRead);
        motaba.Permissions.Add(Permissions.AppNote.Create);
        motaba.Permissions.Add(Permissions.Announcement.AppRead);
        motaba.Permissions.Add(Permissions.Tutorial.AppRead);
        motaba.Permissions.Add(Permissions.Complaint.Submit);
        All.Add(motaba);

        // قوالب لوحة الإدارة — seed يضيف فقط المفقود؛ لا يمحو Deny ولا يغيّر حسابات موجودة
        All.Add(new RolePermissionDto
        {
            RoleName = Roles.Director.Name,
            Permissions = RoleDefaultMatrix.SeedPermissionsFor(Roles.Director.Name).ToList()
        });
        All.Add(new RolePermissionDto
        {
            RoleName = Roles.Monitor.Name,
            Permissions = RoleDefaultMatrix.SeedPermissionsFor(Roles.Monitor.Name).ToList()
        });
        All.Add(new RolePermissionDto
        {
            RoleName = Roles.SalesManager.Name,
            Permissions = RoleDefaultMatrix.SeedPermissionsFor(Roles.SalesManager.Name).ToList()
        });
    }
}

public record RolePermissionDto
{
    public required string RoleName { get; set; }
    public List<string> Permissions { get; set; } = [];
}