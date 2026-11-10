namespace Domain.Entities.LegalFollowupAggregate.Enums;

public enum LegalFollowupStatus
{
    InProgress = 0,
    Completed = 1,
    Cancelled = 2
}

public static class LegalFollowupStatusLabels
{
    public static string Arabic(LegalFollowupStatus status) => status switch
    {
        LegalFollowupStatus.Completed => "تمت المتابعة",
        LegalFollowupStatus.Cancelled => "ملغاة",
        _ => "قيد العمل"
    };
}
