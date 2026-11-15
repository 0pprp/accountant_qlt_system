namespace Application.ManagementTasks.Common;

/// <summary>جمهور إرسال المهمة من اللوحة — نسخ شخصية لكل مستلم.</summary>
public enum ManagementTaskAudience
{
    User = 0,
    Delegate = 1,
    Committee = 2,
    SalesEmployee = 3,
    AllFieldApps = 4,
}

public sealed record ManagementTaskAudienceRecipient(
    int UserId,
    string DisplayName,
    int BranchId);

public sealed record ManagementTaskFanoutResultDto(
    int CreatedCount,
    IReadOnlyList<int> TaskIds);

public sealed record ManagementTaskAudiencePreviewDto(
    ManagementTaskAudience Audience,
    int RecipientCount,
    IReadOnlyList<ManagementTaskAudienceRecipient> Sample);
