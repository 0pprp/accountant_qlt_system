using Application.Common.Models;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Queries.Admin.List;

public record GetAdminSalesRequestsQuery : IRequest<Result<PaginatedList<AdminSalesRequestListItem>>>
{
    public required Pagination Pagination { get; init; }
    public string Tab { get; init; } = "unassigned";
    public int? BranchId { get; init; }
    public int? ProvinceId { get; init; }
    public int? EmployeeId { get; init; }
    public SalesRequestSourceType? SourceType { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public string? Search { get; init; }
    public SalesRequestExceptionHoldStatus? ExceptionStatus { get; init; }
}

public record AdminSalesRequestListItem
{
    public int Id { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public string? SubmissionNote { get; init; }
    public SalesRequestSourceType SourceType { get; init; }
    public required string SourceTypeLabel { get; init; }
    public string? OriginalExternalId { get; init; }
    public int BranchId { get; init; }
    public string? BranchName { get; init; }
    public int? ProvinceId { get; init; }
    public string? ProvinceName { get; init; }
    public SalesRequestStatus Status { get; init; }
    public required string StatusLabel { get; init; }
    public required string TabKey { get; init; }
    public int? TargetEmployeeId { get; init; }
    public string? TargetEmployeeName { get; init; }
    public int? LinkedOrderId { get; init; }
    public int? ExistingCustomerId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? StatusChangedAtUtc { get; init; }
    public string? LastStatusNote { get; init; }
    public string? RejectReason { get; init; }
    public string? CreatedByName { get; init; }
    public int ConcurrencyStamp { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAtUtc { get; init; }
    public string? DeleteReason { get; init; }
    public string? ExceptionState { get; init; }
    public DateTimeOffset? ExceptionDecidedAtUtc { get; init; }
    public DateTimeOffset? ExceptionConsumedAtUtc { get; init; }
    public bool IsSentSource { get; init; }
}
