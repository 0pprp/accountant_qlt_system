namespace Application.SalesRequests.Common;

public static class SalesRequestMatchBatchLimits
{
    /// <summary>الحد المعلن لدفعة ملخص المطابقة — التجاوز خطأ صريح وليس قصًّا صامتًا.</summary>
    public const int MaxRequestIds = 50;
}
