using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Common;

public static class SalesRequestSourceLabels
{
    public static string For(SalesRequestSourceType source) => source switch
    {
        SalesRequestSourceType.Manual => "إدخال يدوي",
        SalesRequestSourceType.EmployeeSubmitted => "موظف المبيعات",
        SalesRequestSourceType.Follower => "مسؤول اللجنة",
        SalesRequestSourceType.Delegate => "مندوب",
        SalesRequestSourceType.ExcelImport => "استيراد Excel",
        SalesRequestSourceType.ExternalCrm => "CRM خارجي",
        _ => source.ToString()
    };

    /// <summary>
    /// مصادر مدعومة في المخزن الحالي. أي مصدر قديم غير مدرج هنا = غير مربوط.
    /// </summary>
    public static readonly SalesRequestSourceType[] Supported =
    [
        SalesRequestSourceType.Manual,
        SalesRequestSourceType.EmployeeSubmitted,
        SalesRequestSourceType.Follower,
        SalesRequestSourceType.Delegate,
        SalesRequestSourceType.ExcelImport,
        SalesRequestSourceType.ExternalCrm
    ];
}
