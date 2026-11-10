using System.Linq.Expressions;
using Application.SalesRequests.Queries.Admin.List;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;

namespace Application.SalesRequests.Common;

public sealed class AdminSalesRequestRow
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhone { get; init; }
    public string? Address { get; init; }
    public string? RequestedProduct { get; init; }
    public string? SubmissionNote { get; init; }
    public SalesRequestSourceType SourceType { get; init; }
    public string? OriginalExternalId { get; init; }
    public int BranchId { get; init; }
    public string? BranchName { get; init; }
    public int? ProvinceId { get; init; }
    public string? ProvinceName { get; init; }
    public SalesRequestStatus Status { get; init; }
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
    public SalesRequestExceptionHoldStatus? ExceptionHoldStatus { get; init; }
    public DateTimeOffset? ExceptionDecidedAtUtc { get; init; }
    public DateTimeOffset? ExceptionConsumedAtUtc { get; init; }
}

public static class AdminSalesRequestRowMapper
{
    public static readonly Expression<Func<SalesRequest, AdminSalesRequestRow>> Projection = x =>
        new AdminSalesRequestRow
        {
            Id = x.Id,
            CustomerName = x.CustomerName,
            CustomerPhone = x.CustomerPhone,
            Address = x.Address,
            RequestedProduct = x.RequestedProduct,
            SubmissionNote = x.SubmissionNote,
            SourceType = x.SourceType,
            OriginalExternalId = x.OriginalExternalId,
            BranchId = x.BranchId,
            BranchName = x.Branch != null ? x.Branch.Name : null,
            ProvinceId = x.ProvinceId,
            ProvinceName = x.Province != null ? x.Province.Name : null,
            Status = x.Status,
            TargetEmployeeId = x.TargetEmployeeId,
            TargetEmployeeName = x.TargetEmployee != null ? x.TargetEmployee.FullName : null,
            LinkedOrderId = x.LinkedOrderId,
            ExistingCustomerId = x.ExistingCustomerId,
            CreatedAt = x.CreatedAt,
            StatusChangedAtUtc = x.StatusChangedAtUtc,
            LastStatusNote = x.LastStatusNote,
            RejectReason = x.RejectReason,
            CreatedByName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null,
            ConcurrencyStamp = x.ConcurrencyStamp,
            IsDeleted = x.IsDeleted,
            DeletedAtUtc = x.DeletedAtUtc,
            DeleteReason = x.DeleteReason,
            ExceptionHoldStatus = x.ExceptionHoldStatus,
            ExceptionDecidedAtUtc = x.ExceptionDecidedAtUtc,
            ExceptionConsumedAtUtc = x.ExceptionConsumedAtUtc
        };

    public static string? ExceptionState(
        SalesRequestExceptionHoldStatus? hold,
        DateTimeOffset? consumedAtUtc)
        => hold switch
        {
            null => null,
            SalesRequestExceptionHoldStatus.Pending => "pending",
            SalesRequestExceptionHoldStatus.Rejected => "rejected",
            _ => consumedAtUtc is null ? "approved" : "approved_consumed"
        };

    public static string TabKey(AdminSalesRequestRow x)
        => x.IsDeleted
            ? SalesRequestTabMap.Deleted
            : SalesRequestAdminQuery.TabKeyFor(
                x.Status, x.TargetEmployeeId, x.ExceptionHoldStatus, x.ExceptionConsumedAtUtc);

    public static AdminSalesRequestListItem ToItem(AdminSalesRequestRow x)
    {
        var tabKey = TabKey(x);
        var isUnassigned = tabKey == SalesRequestTabMap.Unassigned;

        return new AdminSalesRequestListItem
        {
            Id = x.Id,
            CustomerName = x.CustomerName,
            CustomerPhone = x.CustomerPhone,
            Address = x.Address,
            RequestedProduct = x.RequestedProduct,
            SubmissionNote = x.SubmissionNote,
            SourceType = x.SourceType,
            SourceTypeLabel = SalesRequestSourceLabels.For(x.SourceType),
            OriginalExternalId = x.OriginalExternalId,
            BranchId = x.BranchId,
            BranchName = x.BranchName,
            ProvinceId = x.ProvinceId,
            ProvinceName = x.ProvinceName,
            Status = x.Status,
            StatusLabel = isUnassigned ? "غير مسند" : SalesRequestStatusLabels.For(x.Status),
            TabKey = tabKey,
            TargetEmployeeId = x.TargetEmployeeId,
            TargetEmployeeName = x.TargetEmployeeName,
            LinkedOrderId = x.LinkedOrderId,
            ExistingCustomerId = x.ExistingCustomerId,
            CreatedAt = x.CreatedAt,
            StatusChangedAtUtc = x.StatusChangedAtUtc,
            LastStatusNote = x.LastStatusNote,
            RejectReason = x.RejectReason,
            CreatedByName = x.CreatedByName,
            ConcurrencyStamp = x.ConcurrencyStamp,
            IsDeleted = x.IsDeleted,
            DeletedAtUtc = x.DeletedAtUtc,
            DeleteReason = x.DeleteReason,
            ExceptionState = ExceptionState(x.ExceptionHoldStatus, x.ExceptionConsumedAtUtc),
            ExceptionDecidedAtUtc = x.ExceptionDecidedAtUtc,
            ExceptionConsumedAtUtc = x.ExceptionConsumedAtUtc,
            IsSentSource = SalesRequestTabMap.SentSourceTypes.Contains(x.SourceType)
        };
    }
}
