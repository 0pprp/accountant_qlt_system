using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Common;

/// <summary>DTO موظف — requestId منفصل عن linkedOrderId.</summary>
public record SalesRequestAppItemDto
{
    public int RequestId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public string? SubmissionNote { get; init; }
    public SalesRequestStatus Status { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public string TabKey { get; init; } = string.Empty;
    public SalesRequestSourceType SourceType { get; init; }
    public string SourceTypeLabel { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public string? BranchName { get; init; }
    public int? TargetEmployeeId { get; init; }
    public int? LinkedOrderId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? StatusChangedAtUtc { get; init; }
    public DateTimeOffset? ViewedAtUtc { get; init; }
    public string? LastStatusNote { get; init; }
    public bool IsReturned { get; init; }
}
