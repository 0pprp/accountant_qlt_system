namespace Domain.Entities.WaiverAggregate.Enums;

public enum WaiverRequestStatus : byte
{
    Pending = 1,
    AwaitingAccountant = 2,
    Approved = 3,
    Rejected = 4
}

public static class WaiverRequestStatusLabels
{
    public static string Arabic(WaiverRequestStatus s) => s switch
    {
        WaiverRequestStatus.Pending => "قيد مراجعة المفوض",
        WaiverRequestStatus.AwaitingAccountant => "بانتظار المحاسب",
        WaiverRequestStatus.Approved => "مطبّق",
        WaiverRequestStatus.Rejected => "مرفوض",
        _ => s.ToString()
    };
}
