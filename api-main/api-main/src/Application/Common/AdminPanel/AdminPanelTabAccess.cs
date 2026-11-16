using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.Common.AdminPanel;

/// <summary>فحص مفاتيح التبويب على الهوية الحالية (بعد توسيع JWT أو مباشرة).</summary>
public static class AdminPanelTabAccess
{
    public static bool HasTab(ICurrentUserService user, string tabKey) =>
        user.HasPermission(AdminPanelTabs.PermissionName(tabKey));

    public static bool HasAnyTab(ICurrentUserService user, params string[] tabKeys) =>
        tabKeys.Any(k => HasTab(user, k));

    /// <summary>
    /// للمبيعات الخاطئة: فرع = unreviewed، مفوض = unread.
    /// لا يُسمح بتجاوز صندوق بتبويب الآخر.
    /// </summary>
    public static bool CanAccessWrongSaleMailbox(ICurrentUserService user, string? mailbox)
    {
        var m = (mailbox ?? "").Trim().ToLowerInvariant();
        return m switch
        {
            "unreviewed" => HasTab(user, "app-branch-wrongSalesBranch"),
            "unread" => HasTab(user, "app-delegate-wrongSalesDelegate"),
            _ => HasAnyTab(user, "app-branch-wrongSalesBranch", "app-delegate-wrongSalesDelegate")
                 || user.HasPermission("WrongSale.Read")
        };
    }
}
