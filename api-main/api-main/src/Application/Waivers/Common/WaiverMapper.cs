using Domain.Entities.WaiverAggregate;
using Domain.Entities.WaiverAggregate.Enums;

namespace Application.Waivers.Common;

public static class WaiverMapper
{
    public static WaiverSummaryDto ToSummary(
        CustomerWaiverRequest w,
        string customerName,
        string? phone,
        string? branchName) => new(
        w.Id,
        w.CustomerId,
        customerName,
        phone,
        w.BranchId,
        branchName,
        w.OrderId,
        w.Status.ToString(),
        WaiverRequestStatusLabels.Arabic(w.Status),
        w.RequestNote,
        w.RequesterName,
        w.ApprovedAmount,
        w.AppliedAmount,
        w.Applied,
        w.CreatedAtUtc,
        w.DecidedAtUtc,
        w.AppliedAtUtc,
        w.ConcurrencyStamp);

    public static WaiverDetailDto ToDetail(
        CustomerWaiverRequest w,
        string customerName,
        string? phone,
        string? branchName,
        double? sell,
        double? paid,
        double? remaining) => new(
        w.Id,
        w.CustomerId,
        customerName,
        phone,
        w.BranchId,
        branchName,
        w.OrderId,
        sell,
        paid,
        remaining,
        w.Status.ToString(),
        WaiverRequestStatusLabels.Arabic(w.Status),
        w.RequestNote,
        w.RequesterUserId,
        w.RequesterName,
        w.DecisionNote,
        w.DecidedByName,
        w.ApprovedAmount,
        w.AppliedAmount,
        w.Applied,
        w.AppliedByName,
        w.CreatedAtUtc,
        w.DecidedAtUtc,
        w.AppliedAtUtc,
        w.ConcurrencyStamp);
}
