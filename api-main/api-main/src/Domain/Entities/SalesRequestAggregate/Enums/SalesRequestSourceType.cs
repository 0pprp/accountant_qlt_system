namespace Domain.Entities.SalesRequestAggregate.Enums;

/// <summary>
/// مصادر الطلب المدعومة في النموذج الجديد.
/// أي مصدر قديم غير مدرج هنا يُوثَّق كغير مربوط.
/// </summary>
public enum SalesRequestSourceType : byte
{
    Manual = 0,
    EmployeeSubmitted = 1,
    Follower = 2,
    Delegate = 3,
    ExcelImport = 4,
    ExternalCrm = 5,
}
