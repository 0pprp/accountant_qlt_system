using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Domain.Services.SalesRequestLifecycle;

public enum ExceptionDecisionCheck
{
    Allowed,
    NotPending,
    TerminalStatus,
    RequesterUnknown,
    SelfApproval
}

public enum AssignExceptionGate
{
    Free,
    ConsumesApprovedHold,
    BlockedPending,
    BlockedRejected
}

public enum SoftDeleteCheck
{
    Allowed,
    AlreadyDeleted,
    Completed,
    LinkedOrder
}

public enum RestoreCheck
{
    Allowed,
    NotDeleted,
    LinkedOrderAssigneeIneligible
}

public static class SalesRequestExtraViewRules
{
    public static bool IsTerminal(SalesRequestStatus status)
        => status is SalesRequestStatus.Completed
            or SalesRequestStatus.Rejected
            or SalesRequestStatus.LegalFollowup;

    public static bool IsInExceptionsView(SalesRequest request)
        => request.IsDeleted == false
           && request.HasLiveExceptionHold
           && IsTerminal(request.Status) == false;

    public static bool IsInSentView(SalesRequest request)
        => request.IsDeleted == false
           && SalesRequestTabMap.SentSourceTypes.Contains(request.SourceType);

    public static bool IsInDeletedView(SalesRequest request)
        => request.IsDeleted;

    public static ExceptionDecisionCheck CheckDecision(
        SalesRequest request,
        int deciderUserId,
        int? requesterUserId,
        bool approve)
    {
        if (request.IsDeleted || request.ExceptionHoldStatus != SalesRequestExceptionHoldStatus.Pending)
            return ExceptionDecisionCheck.NotPending;

        if (IsTerminal(request.Status))
            return ExceptionDecisionCheck.TerminalStatus;

        if (approve == false)
            return ExceptionDecisionCheck.Allowed;

        if (requesterUserId is null or <= 0)
            return ExceptionDecisionCheck.RequesterUnknown;

        if (requesterUserId == deciderUserId)
            return ExceptionDecisionCheck.SelfApproval;

        return ExceptionDecisionCheck.Allowed;
    }

    public static AssignExceptionGate AssignGate(SalesRequest request)
        => request.ExceptionHoldStatus switch
        {
            SalesRequestExceptionHoldStatus.Pending => AssignExceptionGate.BlockedPending,
            SalesRequestExceptionHoldStatus.Rejected => AssignExceptionGate.BlockedRejected,
            SalesRequestExceptionHoldStatus.Approved when request.ExceptionConsumedAtUtc is null
                => AssignExceptionGate.ConsumesApprovedHold,
            _ => AssignExceptionGate.Free
        };

    public static SoftDeleteCheck CheckSoftDelete(SalesRequest request)
    {
        if (request.IsDeleted)
            return SoftDeleteCheck.AlreadyDeleted;

        if (request.Status == SalesRequestStatus.Completed)
            return SoftDeleteCheck.Completed;

        if (request.LinkedOrderId is > 0)
            return SoftDeleteCheck.LinkedOrder;

        return SoftDeleteCheck.Allowed;
    }

    public static RestoreCheck CheckRestore(SalesRequest request, bool assigneeEligible)
    {
        if (request.IsDeleted == false)
            return RestoreCheck.NotDeleted;

        if (request.LinkedOrderId is > 0
            && IsTerminal(request.Status) == false
            && request.TargetEmployeeId is > 0
            && assigneeEligible == false)
        {
            return RestoreCheck.LinkedOrderAssigneeIneligible;
        }

        return RestoreCheck.Allowed;
    }
}
