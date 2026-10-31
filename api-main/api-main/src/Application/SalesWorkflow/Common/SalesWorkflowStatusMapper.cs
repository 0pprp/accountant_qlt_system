using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;

namespace Application.SalesWorkflow.Common;

public static class SalesWorkflowStatusMapper
{
    public const string Pending = "pending";
    public const string Ready = "ready";
    public const string Disclosure = "disclosure";
    public const string OnHold = "on_hold";
    public const string Rejected = "rejected";
    public const string Sold = "sold";

    public static readonly string[] All =
    [
        Pending,
        Ready,
        Disclosure,
        OnHold,
        Rejected,
        Sold
    ];

    public static string ToApiStatus(Order order)
    {
        if (order.ApprovalStatus == OrderApprovalStatus.Rejected || order.WorkflowStatus == SalesWorkflowStatus.Rejected)
            return Rejected;

        if (order.WorkflowStatus == SalesWorkflowStatus.Sold || order.Step == OrderStep.Completed)
            return Sold;

        return order.WorkflowStatus switch
        {
            SalesWorkflowStatus.Ready => Ready,
            SalesWorkflowStatus.Disclosure => Disclosure,
            SalesWorkflowStatus.OnHold => OnHold,
            SalesWorkflowStatus.Pending => Pending,
            _ => order.Step == OrderStep.SellerInfo ? Ready : Pending
        };
    }

    public static string ToApiStatus(SalesWorkflowStatus status, OrderApprovalStatus approvalStatus, OrderStep step)
    {
        if (approvalStatus == OrderApprovalStatus.Rejected || status == SalesWorkflowStatus.Rejected)
            return Rejected;

        if (status == SalesWorkflowStatus.Sold || step == OrderStep.Completed)
            return Sold;

        return status switch
        {
            SalesWorkflowStatus.Ready => Ready,
            SalesWorkflowStatus.Disclosure => Disclosure,
            SalesWorkflowStatus.OnHold => OnHold,
            SalesWorkflowStatus.Pending => Pending,
            _ => step == OrderStep.SellerInfo ? Ready : Pending
        };
    }

    public static bool TryParse(string? status, out string normalized)
    {
        normalized = (status ?? string.Empty).Trim().ToLowerInvariant().Replace('-', '_');
        return All.Contains(normalized);
    }
}
