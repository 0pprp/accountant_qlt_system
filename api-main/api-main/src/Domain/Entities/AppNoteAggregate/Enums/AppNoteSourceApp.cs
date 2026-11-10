namespace Domain.Entities.AppNoteAggregate.Enums;

/// <summary>مصدر التطبيق الموثوق من السيرفر — ليس قيمة يختارها العميل بحرية.</summary>
public enum AppNoteSourceApp : byte
{
    Follower = 1,
    Delegate = 2,
    BranchManager = 3,
    /// <summary>موظف مبيعات مكلّف — ليس مندوب جباية لمجرد دور Mandob.</summary>
    SalesEmployee = 4
}
