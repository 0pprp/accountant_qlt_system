using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.OrderListAggregate;

public class OrderList : AuditableEntity
{
    public OrderList(string name, int mandobId, int motabaId, int branchId)
    {
        Name = name;
        MandobId = mandobId;
        MotabaId = motabaId;
        BranchId = branchId;
        Orders = new HashSet<Order>();
    }

    public string Name { get; set; }
    public int MandobId { get; set; }
    public int MotabaId { get; set; }
    public int BranchId { get; set; }

    /// <summary>تمكين جباية المتابع على هذه القائمة — يمنع جباية المندوب.</summary>
    public bool FollowerCollectionEnabled { get; private set; }

    /// <summary>وقت آخر تفعيل للجباية (بغداد/UTC مخزّن كـ Offset).</summary>
    public DateTimeOffset? FollowerCollectionAssignedAtUtc { get; private set; }

    /// <summary>نسخة تبديل التمكين للتزامن.</summary>
    public int CollectionToggleVersion { get; private set; }

    public Branch? Branch { get; set; }
    public User? Mandob { get; set; }
    public User? Motaba { get; set; }
    public ICollection<Order> Orders { get; set; }

    public void Update(string name, int mandobId, int motabaId, int branchId)
    {
        Name = name;
        MandobId = mandobId;
        MotabaId = motabaId;
        BranchId = branchId;
    }

    /// <returns>false إذا الحالة المطلوبة هي الحالية أصلًا (لا أثر جديد ولا رفع للنسخة).</returns>
    public bool SetFollowerCollectionEnabled(bool enabled, DateTimeOffset atUtc, int? expectedVersion)
    {
        if (expectedVersion is int v && v != CollectionToggleVersion)
            throw new InvalidOperationException("COLLECTION_VERSION_CONFLICT");

        if (FollowerCollectionEnabled == enabled)
            return false;

        FollowerCollectionEnabled = enabled;
        if (enabled)
            FollowerCollectionAssignedAtUtc = atUtc;
        CollectionToggleVersion++;
        return true;
    }
}