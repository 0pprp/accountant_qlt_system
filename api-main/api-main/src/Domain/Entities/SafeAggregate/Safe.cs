using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SafeAggregate;

/// <summary>
/// خزينة نقد تابعة لفرع. الرصيد لا يُعدَّل إلا عبر <see cref="TryApplyDelta"/> من طبقة الـledger،
/// وكل تعديل يرافقه صف في <see cref="SafeMovement"/> فيبقى الرصيد قابلاً للتحقق من السجل.
/// </summary>
public class Safe : AuditableEntity
{
    /// <summary>هامش مقارنة لأن المبالغ double؛ أصغر دينار متعامل به هو 1000.</summary>
    public const double Tolerance = 0.001;

    private Safe()
    {
        Name = string.Empty;
        Movements = new HashSet<SafeMovement>();
    }

    private Safe(string name, SafeKind kind, int branchId, int? orderListId, int? userId, bool isSystem)
    {
        Name = name;
        Kind = kind;
        BranchId = branchId;
        OrderListId = orderListId;
        UserId = userId;
        IsSystem = isSystem;
        IsActive = true;
        Balance = 0;
        Movements = new HashSet<SafeMovement>();
    }

    /// <summary>قاصة الفرع — خزينة النظام الرئيسية للفرع، لا تُحذف.</summary>
    public static Safe ForBranch(string branchName, int branchId) =>
        new(SafeNames.ForBranch(branchName), SafeKind.Branch, branchId, null, null, true);

    /// <summary>سجل الصرفيات — تستقبل كل سحب من خزائن الفرع، لا تُحذف.</summary>
    public static Safe ForExpenses(string branchName, int branchId) =>
        new(SafeNames.ForExpenses(branchName), SafeKind.Expenses, branchId, null, null, true);

    /// <summary>خزينة قائمة تحصيل — عهدة المندوب تتبع القائمة لا الشخص.</summary>
    public static Safe ForOrderList(string orderListName, int branchId, int orderListId) =>
        new(SafeNames.ForOrderList(orderListName), SafeKind.OrderList, branchId, orderListId, null, false);

    /// <summary>خزينة موظف — المتابع المُحصِّل وموظف المبيعات مستلم المقدمة.</summary>
    public static Safe ForEmployee(string employeeName, int branchId, int userId) =>
        new(SafeNames.ForEmployee(employeeName), SafeKind.Employee, branchId, null, userId, false);

    /// <summary>خزينة متفرقة — ذمة أو مكتب أو صندوق لا يتبع قائمة ولا موظفاً.</summary>
    public static Safe ForMisc(string safeName, int branchId) =>
        new(SafeNames.ForMisc(safeName), SafeKind.Misc, branchId, null, null, false);

    public string Name { get; private set; }
    public SafeKind Kind { get; private set; }
    public int BranchId { get; private set; }

    /// <summary>القائمة المربوطة — مضبوطة فقط عند <see cref="SafeKind.OrderList"/>.</summary>
    public int? OrderListId { get; private set; }

    /// <summary>الموظف المربوط — مضبوط فقط عند <see cref="SafeKind.Employee"/>.</summary>
    public int? UserId { get; private set; }

    public double Balance { get; private set; }

    /// <summary>خزينة نظام (فرع أو صرفيات) — لا تُحذف ولا يُعاد ربطها.</summary>
    public bool IsSystem { get; private set; }

    public bool IsActive { get; private set; }

    public Branch? Branch { get; set; }
    public OrderList? OrderList { get; set; }
    public User? User { get; set; }
    public ICollection<SafeMovement> Movements { get; set; }

    public bool IsEmpty => Math.Abs(Balance) < Tolerance;

    /// <summary>الحذف مسموح للخزائن غير النظامية الخالية من الرصيد؛ وجود حركات يُفحص في الـhandler.</summary>
    public bool CanBeDeleted => IsSystem == false && IsEmpty;

    public void Rename(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0 || trimmed == Name)
            return;

        Name = trimmed.Length <= SafeNames.MaxLength ? trimmed : trimmed[..SafeNames.MaxLength];
    }

    public void Update(string name, bool isActive)
    {
        Rename(name);
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// يطبّق تغييراً موقّعاً على الرصيد ويُرجع الرصيد الجديد، أو null إن أدّى إلى رصيد سالب فلا يتغير شيء.
    /// لا يُنادى إلا من الـledger داخل معاملة تحمل قفل الخزينة.
    /// </summary>
    public double? TryApplyDelta(double amount)
    {
        var next = Balance + amount;
        if (next < -Tolerance)
            return null;

        Balance = next;
        return Balance;
    }
}
