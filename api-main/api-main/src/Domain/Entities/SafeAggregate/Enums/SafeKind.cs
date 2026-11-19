namespace Domain.Entities.SafeAggregate.Enums;

public enum SafeKind : byte
{
    Branch = 0,
    Expenses = 1,
    OrderList = 2,
    Employee = 3,

    /// <summary>خزينة متفرقة — ذمم ومكاتب وصناديق لا تتبع قائمة ولا موظفاً. تُنشأ يدوياً ولا يرمّمها المزوّد.</summary>
    Misc = 4
}
