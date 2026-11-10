using System.Security.Claims;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.ApproveException;
using Application.SalesRequests.Commands.Admin.Assign;
using Application.SalesRequests.Commands.Admin.RejectException;
using Application.SalesRequests.Commands.Admin.RequestException;
using Application.SalesRequests.Commands.Admin.Restore;
using Application.SalesRequests.Commands.Admin.SoftDelete;
using Application.SalesRequests.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SalesEmployeeAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

public class SalesRequestExtraViews04ETests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    private static SalesRequest NewRequest(SalesRequestSourceType source = SalesRequestSourceType.Manual)
        => new("علي حسن كاظم", "07801112233", source, 1, null, null, null);

    private static SalesRequest HeldRequest(SalesRequestExceptionHoldStatus hold)
    {
        var request = NewRequest();
        request.HoldForException(SalesRequestExceptionHoldStatus.Pending);
        if (hold == SalesRequestExceptionHoldStatus.Approved)
            request.ApproveException(2, T0);
        if (hold == SalesRequestExceptionHoldStatus.Rejected)
            request.RejectException(2, T0);
        return request;
    }

    [Fact]
    public void Approved_hold_is_consumed_by_first_assign_and_cannot_be_reused()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Approved);

        Assert.True(request.HasLiveExceptionHold);
        Assert.Equal(AssignExceptionGate.ConsumesApprovedHold, SalesRequestExtraViewRules.AssignGate(request));

        request.Assign(7, T0.AddMinutes(1));

        Assert.Equal(T0.AddMinutes(1), request.ExceptionConsumedAtUtc);
        Assert.Equal(SalesRequestExceptionHoldStatus.Approved, request.ExceptionHoldStatus);
        Assert.False(request.HasLiveExceptionHold);
        Assert.Equal(AssignExceptionGate.Free, SalesRequestExtraViewRules.AssignGate(request));
        Assert.False(SalesRequestExtraViewRules.IsInExceptionsView(request));
        Assert.False(request.ConsumeApprovedException(T0.AddMinutes(2)));
        Assert.Equal(T0.AddMinutes(1), request.ExceptionConsumedAtUtc);
    }

    [Fact]
    public void Consumed_hold_does_not_hide_request_from_unassigned_when_assignee_is_removed()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Approved);
        request.Assign(7, T0);
        request.TargetEmployeeId = null;
        request.Status = SalesRequestStatus.New;

        Assert.True(request.IsUnassigned());
        Assert.False(request.ConsumeApprovedException(T0.AddMinutes(5)));
    }

    [Fact]
    public void Pending_and_rejected_holds_block_assign_and_reassign()
    {
        var pending = HeldRequest(SalesRequestExceptionHoldStatus.Pending);
        Assert.Equal(AssignExceptionGate.BlockedPending, SalesRequestExtraViewRules.AssignGate(pending));
        Assert.Throws<InvalidOperationException>(() => pending.Assign(7, T0));
        Assert.Throws<InvalidOperationException>(() => pending.Reassign(7, null, T0));

        var rejected = HeldRequest(SalesRequestExceptionHoldStatus.Rejected);
        Assert.Equal(AssignExceptionGate.BlockedRejected, SalesRequestExtraViewRules.AssignGate(rejected));
        Assert.Throws<InvalidOperationException>(() => rejected.Assign(7, T0));
    }

    [Fact]
    public void New_exception_request_after_consumption_resets_consumption_and_needs_new_decision()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Approved);
        request.Assign(7, T0);

        request.HoldForException(SalesRequestExceptionHoldStatus.Pending);

        Assert.Equal(SalesRequestExceptionHoldStatus.Pending, request.ExceptionHoldStatus);
        Assert.Null(request.ExceptionConsumedAtUtc);
        Assert.Null(request.ExceptionDecidedAtUtc);
        Assert.Equal(AssignExceptionGate.BlockedPending, SalesRequestExtraViewRules.AssignGate(request));
    }

    [Fact]
    public void Exception_decision_rules_block_self_approval_and_unknown_requester()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Pending);

        Assert.Equal(
            ExceptionDecisionCheck.SelfApproval,
            SalesRequestExtraViewRules.CheckDecision(request, deciderUserId: 5, requesterUserId: 5, approve: true));
        Assert.Equal(
            ExceptionDecisionCheck.RequesterUnknown,
            SalesRequestExtraViewRules.CheckDecision(request, 5, null, approve: true));
        Assert.Equal(
            ExceptionDecisionCheck.Allowed,
            SalesRequestExtraViewRules.CheckDecision(request, 6, 5, approve: true));
        Assert.Equal(
            ExceptionDecisionCheck.Allowed,
            SalesRequestExtraViewRules.CheckDecision(request, 5, 5, approve: false));
    }

    [Fact]
    public void Exception_decision_requires_pending_hold_and_non_terminal_status()
    {
        var none = NewRequest();
        Assert.Equal(ExceptionDecisionCheck.NotPending, SalesRequestExtraViewRules.CheckDecision(none, 6, 5, true));
        Assert.Throws<InvalidOperationException>(() => none.ApproveException(6, T0));

        var decided = HeldRequest(SalesRequestExceptionHoldStatus.Approved);
        Assert.Equal(ExceptionDecisionCheck.NotPending, SalesRequestExtraViewRules.CheckDecision(decided, 6, 5, true));
        Assert.Throws<InvalidOperationException>(() => decided.RejectException(6, T0));

        var terminal = HeldRequest(SalesRequestExceptionHoldStatus.Pending);
        terminal.Status = SalesRequestStatus.Rejected;
        Assert.Equal(ExceptionDecisionCheck.TerminalStatus, SalesRequestExtraViewRules.CheckDecision(terminal, 6, 5, true));
    }

    [Fact]
    public void Exception_decision_records_decider_and_bumps_concurrency_stamp()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Pending);
        var stamp = request.ConcurrencyStamp;

        request.ApproveException(9, T0);

        Assert.Equal(9, request.ExceptionDecidedByUserId);
        Assert.Equal(T0, request.ExceptionDecidedAtUtc);
        Assert.Null(request.ExceptionConsumedAtUtc);
        Assert.Equal(stamp + 1, request.ConcurrencyStamp);
    }

    [Fact]
    public void Exception_decide_permission_is_separate_from_request_exception()
    {
        Assert.NotEqual(SeedPermissions.SalesRequest.RequestException, SeedPermissions.SalesRequest.ExceptionDecide);

        var names = SeedPermissions.SalesRequest.OutsideAll.Select(x => x.Name).ToList();
        Assert.Contains(SeedPermissions.SalesRequest.RequestException, names);
        Assert.Contains(SeedPermissions.SalesRequest.ExceptionDecide, names);
        Assert.Contains(SeedPermissions.SalesRequest.TabSent, names);
        Assert.Contains(SeedPermissions.SalesRequest.TabExceptions, names);
        Assert.Contains(SeedPermissions.SalesRequest.TabDeleted, names);
        Assert.Contains(SeedPermissions.SalesRequest.SoftDelete, names);
        Assert.Contains(SeedPermissions.SalesRequest.Restore, names);
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void Soft_delete_requires_reason_keeps_status_and_records_audit_fields()
    {
        var request = NewRequest();
        request.Assign(7, T0);
        request.Pend("انتظار", T0);

        Assert.Throws<InvalidOperationException>(() => request.SoftDelete("  ", 3, T0));

        request.SoftDelete("  مكرر  ", 3, T0.AddHours(1));

        Assert.True(request.IsDeleted);
        Assert.Equal("مكرر", request.DeleteReason);
        Assert.Equal(3, request.DeletedByUserId);
        Assert.Equal(T0.AddHours(1), request.DeletedAtUtc);
        Assert.Equal(SalesRequestStatus.Pending, request.Status);
        Assert.Equal(7, request.TargetEmployeeId);
        Assert.False(request.IsUnassigned());
        Assert.False(request.IsOwnedBy(7));
        Assert.True(SalesRequestExtraViewRules.IsInDeletedView(request));
        Assert.Throws<InvalidOperationException>(() => request.SoftDelete("مرة ثانية", 3, T0));
    }

    [Fact]
    public void Soft_delete_rules_block_completed_linked_and_already_deleted()
    {
        var completed = NewRequest();
        completed.Status = SalesRequestStatus.Completed;
        Assert.Equal(SoftDeleteCheck.Completed, SalesRequestExtraViewRules.CheckSoftDelete(completed));

        var linked = NewRequest();
        linked.Assign(7, T0);
        linked.LinkDraftOrder(50, T0);
        Assert.Equal(SoftDeleteCheck.LinkedOrder, SalesRequestExtraViewRules.CheckSoftDelete(linked));

        var deleted = NewRequest();
        deleted.SoftDelete("x", 3, T0);
        Assert.Equal(SoftDeleteCheck.AlreadyDeleted, SalesRequestExtraViewRules.CheckSoftDelete(deleted));

        Assert.Equal(SoftDeleteCheck.Allowed, SalesRequestExtraViewRules.CheckSoftDelete(NewRequest()));
    }

    [Fact]
    public void Restore_preserves_status_and_owner_when_assignee_still_eligible()
    {
        var request = NewRequest();
        request.Assign(7, T0);
        request.Pend("انتظار", T0);
        request.SoftDelete("خطأ", 3, T0);

        var outcome = request.Restore(assigneeEligible: true, T0.AddDays(1));

        Assert.False(request.IsDeleted);
        Assert.Null(request.DeletedAtUtc);
        Assert.Null(request.DeleteReason);
        Assert.Equal(SalesRequestStatus.Pending, request.Status);
        Assert.Equal(7, request.TargetEmployeeId);
        Assert.False(outcome.AssigneeCleared);
        Assert.False(outcome.ApprovedHoldDowngraded);
    }

    [Fact]
    public void Restore_does_not_revive_ineligible_assignee_and_returns_request_to_unassigned()
    {
        var request = NewRequest();
        request.Assign(7, T0);
        request.MarkViewed(T0);
        request.SoftDelete("خطأ", 3, T0);

        var outcome = request.Restore(assigneeEligible: false, T0.AddDays(1));

        Assert.True(outcome.AssigneeCleared);
        Assert.Null(request.TargetEmployeeId);
        Assert.Equal(SalesRequestStatus.New, request.Status);
        Assert.Null(request.ViewedAtUtc);
        Assert.True(request.IsUnassigned());
    }

    [Theory]
    [InlineData(SalesRequestStatus.Completed)]
    [InlineData(SalesRequestStatus.Rejected)]
    [InlineData(SalesRequestStatus.LegalFollowup)]
    public void Restore_keeps_terminal_status_and_never_reopens_it(SalesRequestStatus terminal)
    {
        var request = NewRequest();
        request.Assign(7, T0);
        request.Status = terminal;
        request.SoftDelete("x", 3, T0);

        var outcome = request.Restore(assigneeEligible: false, T0.AddDays(1));

        Assert.Equal(terminal, request.Status);
        Assert.Equal(7, request.TargetEmployeeId);
        Assert.False(outcome.AssigneeCleared);
        Assert.False(request.IsUnassigned());
    }

    [Fact]
    public void Restore_downgrades_unconsumed_approved_exception_to_pending()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Approved);
        request.SoftDelete("x", 3, T0);

        var outcome = request.Restore(assigneeEligible: true, T0.AddDays(1));

        Assert.True(outcome.ApprovedHoldDowngraded);
        Assert.Equal(SalesRequestExceptionHoldStatus.Pending, request.ExceptionHoldStatus);
        Assert.Null(request.ExceptionDecidedByUserId);
        Assert.Equal(AssignExceptionGate.BlockedPending, SalesRequestExtraViewRules.AssignGate(request));
    }

    [Fact]
    public void Restore_keeps_consumed_approved_exception_consumed()
    {
        var request = HeldRequest(SalesRequestExceptionHoldStatus.Approved);
        request.Assign(7, T0);
        var consumedAt = request.ExceptionConsumedAtUtc;
        request.SoftDelete("x", 3, T0);

        var outcome = request.Restore(assigneeEligible: true, T0.AddDays(1));

        Assert.False(outcome.ApprovedHoldDowngraded);
        Assert.Equal(consumedAt, request.ExceptionConsumedAtUtc);
        Assert.False(request.HasLiveExceptionHold);
    }

    [Fact]
    public void Restore_rules_require_deleted_and_block_linked_request_with_ineligible_assignee()
    {
        Assert.Equal(RestoreCheck.NotDeleted, SalesRequestExtraViewRules.CheckRestore(NewRequest(), true));
        Assert.Throws<InvalidOperationException>(() => NewRequest().Restore(true, T0));

        var linked = NewRequest();
        linked.Assign(7, T0);
        linked.LinkedOrderId = 50;
        linked.Status = SalesRequestStatus.ConvertedToSale;
        linked.IsDeleted = true;
        Assert.Equal(
            RestoreCheck.LinkedOrderAssigneeIneligible,
            SalesRequestExtraViewRules.CheckRestore(linked, assigneeEligible: false));
        Assert.Equal(RestoreCheck.Allowed, SalesRequestExtraViewRules.CheckRestore(linked, assigneeEligible: true));
    }

    [Fact]
    public void Sent_view_uses_person_submitted_sources_only_and_none_have_ingestion()
    {
        Assert.Equal(
            [SalesRequestSourceType.EmployeeSubmitted, SalesRequestSourceType.Follower, SalesRequestSourceType.Delegate],
            SalesRequestTabMap.SentSourceTypes);
        Assert.Empty(SalesRequestTabMap.IngestionWiredSourceTypes);

        Assert.True(SalesRequestExtraViewRules.IsInSentView(NewRequest(SalesRequestSourceType.Follower)));
        Assert.False(SalesRequestExtraViewRules.IsInSentView(NewRequest(SalesRequestSourceType.Manual)));
        Assert.False(SalesRequestExtraViewRules.IsInSentView(NewRequest(SalesRequestSourceType.ExcelImport)));
        Assert.False(SalesRequestExtraViewRules.IsInSentView(NewRequest(SalesRequestSourceType.ExternalCrm)));

        var deleted = NewRequest(SalesRequestSourceType.Delegate);
        deleted.SoftDelete("x", 3, T0);
        Assert.False(SalesRequestExtraViewRules.IsInSentView(deleted));
    }

    [Fact]
    public void Exceptions_view_lists_live_holds_for_non_terminal_requests_only()
    {
        Assert.True(SalesRequestExtraViewRules.IsInExceptionsView(HeldRequest(SalesRequestExceptionHoldStatus.Pending)));
        Assert.True(SalesRequestExtraViewRules.IsInExceptionsView(HeldRequest(SalesRequestExceptionHoldStatus.Approved)));
        Assert.True(SalesRequestExtraViewRules.IsInExceptionsView(HeldRequest(SalesRequestExceptionHoldStatus.Rejected)));
        Assert.False(SalesRequestExtraViewRules.IsInExceptionsView(NewRequest()));

        var rejectedRequest = HeldRequest(SalesRequestExceptionHoldStatus.Pending);
        rejectedRequest.Status = SalesRequestStatus.Rejected;
        Assert.False(SalesRequestExtraViewRules.IsInExceptionsView(rejectedRequest));
    }

    [Fact]
    public void Legal_view_is_implemented_and_has_tab_permission()
    {
        Assert.False(SalesRequestTabMap.IsExtraNotImplemented("legal"));
        Assert.True(SalesRequestTabMap.IsExtraImplemented("legal"));
        Assert.Equal(
            SeedPermissions.SalesRequest.TabLegal,
            SalesRequestTabPermissions.PermissionForTab("legal"));
    }

    [Fact]
    public void Tab_permissions_map_extra_views_and_row_visibility_follows_them()
    {
        Assert.Equal(SeedPermissions.SalesRequest.TabSent, SalesRequestTabPermissions.PermissionForTab("sent"));
        Assert.Equal(SeedPermissions.SalesRequest.TabExceptions, SalesRequestTabPermissions.PermissionForTab("exceptions"));
        Assert.Equal(SeedPermissions.SalesRequest.TabDeleted, SalesRequestTabPermissions.PermissionForTab("deleted"));

        var exceptionsOnly = User(1, SeedPermissions.SalesRequest.TabExceptions);
        Assert.True(SalesRequestAdminQuery.CanReadRow(
            exceptionsOnly, false, SalesRequestStatus.New, null,
            SalesRequestExceptionHoldStatus.Pending, null, SalesRequestSourceType.Manual));
        Assert.False(SalesRequestAdminQuery.CanReadRow(
            exceptionsOnly, false, SalesRequestStatus.New, null,
            null, null, SalesRequestSourceType.Manual));
        Assert.False(SalesRequestAdminQuery.CanReadRow(
            exceptionsOnly, true, SalesRequestStatus.New, null,
            null, null, SalesRequestSourceType.Manual));

        var deletedOnly = User(1, SeedPermissions.SalesRequest.TabDeleted);
        Assert.True(SalesRequestAdminQuery.CanReadRow(
            deletedOnly, true, SalesRequestStatus.Assigned, 7,
            null, null, SalesRequestSourceType.Manual));
        Assert.False(SalesRequestAdminQuery.CanReadRow(
            deletedOnly, false, SalesRequestStatus.Assigned, 7,
            null, null, SalesRequestSourceType.Manual));

        var sentOnly = User(1, SeedPermissions.SalesRequest.TabSent);
        Assert.True(SalesRequestAdminQuery.CanReadRow(
            sentOnly, false, SalesRequestStatus.Completed, 7,
            null, null, SalesRequestSourceType.Delegate));
        Assert.False(SalesRequestAdminQuery.CanReadRow(
            sentOnly, false, SalesRequestStatus.Completed, 7,
            null, null, SalesRequestSourceType.Manual));
    }

    [Fact]
    public void Extra_views_do_not_feed_the_all_tab_or_workflow_union()
    {
        var all = User(1, SeedPermissions.SalesRequest.TabSent, SeedPermissions.SalesRequest.TabDeleted);

        Assert.Empty(SalesRequestTabPermissions.AllowedWorkflowTabs(all));
        Assert.Equal(["sent", "deleted"], SalesRequestTabPermissions.AllowedExtraViews(all));
        Assert.True(SalesRequestTabPermissions.HasAnyReadableView(all));
        Assert.DoesNotContain("sent", SalesRequestTabMap.WorkflowTabs);
    }

    [Fact]
    public async Task Handlers_approve_by_different_user_then_assign_consumes_hold_once()
    {
        await using var db = CreateDb();
        var requestId = await SeedHeldRequestAsync(db, requesterUserId: 5);

        var requesterApprove = await new ApproveSalesRequestExceptionHandler(
            db, Admin(5, SeedPermissions.SalesRequest.ExceptionDecide), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = requestId, Reason = "ok" }, default);
        Assert.True(requesterApprove.IsFailed);
        Assert.Equal(SalesRequestErrors.ExceptionSelfApproval.Code, requesterApprove.Error!.Value.Code);

        var noPermission = await new ApproveSalesRequestExceptionHandler(
            db, Admin(6, SeedPermissions.SalesRequest.RequestException), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = requestId, Reason = "ok" }, default);
        Assert.Equal(SalesRequestErrors.ActionForbidden.Code, noPermission.Error!.Value.Code);

        var blockedAssign = await AssignAsync(db, requestId);
        Assert.Equal(SalesRequestErrors.ExceptionPendingBlocksAssign.Code, blockedAssign.Error!.Value.Code);

        var approved = await new ApproveSalesRequestExceptionHandler(
            db, Admin(6, SeedPermissions.SalesRequest.ExceptionDecide), new ClockStub())
            .Handle(new ApproveSalesRequestExceptionCommand { RequestId = requestId, Reason = "ok" }, default);
        Assert.True(approved.IsSucceeded);

        var first = await AssignAsync(db, requestId);
        Assert.True(first.IsSucceeded);

        var entity = await db.SalesRequests.SingleAsync(x => x.Id == requestId);
        Assert.NotNull(entity.ExceptionConsumedAtUtc);
        Assert.NotNull(entity.TargetEmployeeId);
        Assert.Equal(SalesRequestStatus.Assigned, entity.Status);
        Assert.Contains(
            await db.SalesRequestTimelineEntries.Select(x => x.EventType).ToListAsync(),
            t => t == SalesRequestTimelineEventType.ExceptionConsumed);

        var requestAgain = await new RequestSalesRequestExceptionHandler(
            db, Admin(5, SeedPermissions.SalesRequest.RequestException), new ClockStub())
            .Handle(new RequestSalesRequestExceptionCommand { RequestId = requestId, Reason = "again" }, default);
        Assert.True(requestAgain.IsSucceeded);

        var secondAssign = await AssignAsync(db, requestId);
        Assert.Equal(SalesRequestErrors.ExceptionPendingBlocksAssign.Code, secondAssign.Error!.Value.Code);
    }

    [Fact]
    public async Task Handlers_reject_exception_blocks_assign_and_requires_reason()
    {
        await using var db = CreateDb();
        var requestId = await SeedHeldRequestAsync(db, requesterUserId: 5);
        var decider = Admin(6, SeedPermissions.SalesRequest.ExceptionDecide);

        var noReason = await new RejectSalesRequestExceptionHandler(db, decider, new ClockStub())
            .Handle(new RejectSalesRequestExceptionCommand { RequestId = requestId, Reason = " " }, default);
        Assert.Equal(SalesRequestErrors.NoteRequired.Code, noReason.Error!.Value.Code);

        var rejected = await new RejectSalesRequestExceptionHandler(db, decider, new ClockStub())
            .Handle(new RejectSalesRequestExceptionCommand { RequestId = requestId, Reason = "غير مبرر" }, default);
        Assert.True(rejected.IsSucceeded);

        var assign = await AssignAsync(db, requestId);
        Assert.Equal(SalesRequestErrors.ExceptionRejectedBlocksAssign.Code, assign.Error!.Value.Code);

        var timeline = await db.SalesRequestTimelineEntries
            .SingleAsync(x => x.EventType == SalesRequestTimelineEventType.ExceptionRejected);
        Assert.Equal("غير مبرر", timeline.Reason);
        Assert.Equal(6, timeline.ActorUserId);
    }

    [Fact]
    public async Task Handlers_soft_delete_then_restore_follow_rules_and_write_timeline()
    {
        await using var db = CreateDb();
        var request = NewRequest();
        request.Assign(77, T0);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var user = Admin(3, SeedPermissions.SalesRequest.SoftDelete, SeedPermissions.SalesRequest.Restore);

        var noReason = await new SoftDeleteSalesRequestHandler(db, user, new ClockStub())
            .Handle(new SoftDeleteSalesRequestCommand { RequestId = request.Id, Reason = "" }, default);
        Assert.Equal(SalesRequestErrors.NoteRequired.Code, noReason.Error!.Value.Code);

        var deleted = await new SoftDeleteSalesRequestHandler(db, user, new ClockStub())
            .Handle(new SoftDeleteSalesRequestCommand { RequestId = request.Id, Reason = "تكرار" }, default);
        Assert.True(deleted.IsSucceeded);

        var again = await new SoftDeleteSalesRequestHandler(db, user, new ClockStub())
            .Handle(new SoftDeleteSalesRequestCommand { RequestId = request.Id, Reason = "تكرار" }, default);
        Assert.Equal(SalesRequestErrors.AlreadyDeleted.Code, again.Error!.Value.Code);

        var restored = await new RestoreSalesRequestHandler(db, user, new ClockStub())
            .Handle(new RestoreSalesRequestCommand { RequestId = request.Id, Reason = "رجوع" }, default);
        Assert.True(restored.IsSucceeded);

        var entity = await db.SalesRequests.SingleAsync(x => x.Id == request.Id);
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.TargetEmployeeId);
        Assert.Equal(SalesRequestStatus.New, entity.Status);

        var restoreEntry = await db.SalesRequestTimelineEntries
            .SingleAsync(x => x.EventType == SalesRequestTimelineEventType.Restored);
        Assert.Contains(RestoreSalesRequestHandler.AssigneeClearedMarker, restoreEntry.Reason);
        Assert.Equal(77, restoreEntry.PreviousEmployeeId);
        Assert.Null(restoreEntry.NewEmployeeId);

        var notDeleted = await new RestoreSalesRequestHandler(db, user, new ClockStub())
            .Handle(new RestoreSalesRequestCommand { RequestId = request.Id }, default);
        Assert.Equal(SalesRequestErrors.NotDeleted.Code, notDeleted.Error!.Value.Code);
    }

    [Fact]
    public async Task Handlers_soft_delete_blocks_linked_order_and_missing_permission()
    {
        await using var db = CreateDb();
        var request = NewRequest();
        request.Assign(77, T0);
        request.LinkDraftOrder(50, T0);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var forbidden = await new SoftDeleteSalesRequestHandler(
                db, Admin(3, SeedPermissions.SalesRequest.Restore), new ClockStub())
            .Handle(new SoftDeleteSalesRequestCommand { RequestId = request.Id, Reason = "x" }, default);
        Assert.Equal(SalesRequestErrors.ActionForbidden.Code, forbidden.Error!.Value.Code);

        var linked = await new SoftDeleteSalesRequestHandler(
                db, Admin(3, SeedPermissions.SalesRequest.SoftDelete), new ClockStub())
            .Handle(new SoftDeleteSalesRequestCommand { RequestId = request.Id, Reason = "x" }, default);
        Assert.Equal(SalesRequestErrors.DeleteLinkedOrderForbidden.Code, linked.Error!.Value.Code);

        Assert.False((await db.SalesRequests.SingleAsync()).IsDeleted);
        Assert.Empty(db.SalesRequestChangeOutboxes);
    }

    [Fact]
    public async Task Handlers_restore_keeps_eligible_assignee()
    {
        await using var db = CreateDb();
        var employee = await AddEligibleUserAsync(db, branchId: 1);
        var employeeId = employee.Id;

        var request = NewRequest();
        request.Assign(employeeId, T0);
        request.SoftDelete("x", 3, T0);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var restored = await new RestoreSalesRequestHandler(
                db, Admin(3, SeedPermissions.SalesRequest.Restore), new ClockStub())
            .Handle(new RestoreSalesRequestCommand { RequestId = request.Id }, default);
        Assert.True(restored.IsSucceeded);

        var entity = await db.SalesRequests.SingleAsync();
        Assert.Equal(employeeId, entity.TargetEmployeeId);
        Assert.Equal(SalesRequestStatus.Assigned, entity.Status);
    }

    private static async Task<int> SeedHeldRequestAsync(TestApplicationDbContext db, int requesterUserId)
    {
        await AddEligibleUserAsync(db, branchId: 1);
        await AddEligibleUserAsync(db, branchId: 1);

        var request = NewRequest();
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        var result = await new RequestSalesRequestExceptionHandler(
                db, Admin(requesterUserId, SeedPermissions.SalesRequest.RequestException), new ClockStub())
            .Handle(new RequestSalesRequestExceptionCommand { RequestId = request.Id, Reason = "طلب" }, default);
        Assert.True(result.IsSucceeded);
        return request.Id;
    }

    private static async Task<Result> AssignAsync(TestApplicationDbContext db, int requestId)
    {
        var employee = await db.Users.OrderBy(x => x.Id).FirstAsync();
        return await new AssignSalesRequestHandler(
                db, Admin(4, SeedPermissions.SalesRequest.Assign), new ClockStub())
            .Handle(new AssignSalesRequestCommand { RequestId = requestId, EmployeeId = employee.Id }, default);
    }

    /// <summary>موظف أهل للإسناد: فرع + AppRead (لا عضوية فرع وحدها).</summary>
    private static async Task<Domain.Entities.UserAggregate.User> AddEligibleUserAsync(
        TestApplicationDbContext db, int branchId)
    {
        var appRead = await db.Permissions
            .FirstOrDefaultAsync(p => p.Name == SeedPermissions.SalesRequest.AppRead);
        if (appRead is null)
        {
            appRead = new Permission(
                SeedPermissions.SalesRequest.AppRead,
                "قراءة تطبيق طلبات البيع",
                "SalesRequest",
                "طلبات البيع");
            db.Permissions.Add(appRead);
            await db.SaveChangesAsync();
        }

        var user = new Domain.Entities.UserAggregate.User(
            "موظف", "أم", "123", new DateOnly(1990, 1, 1), $"user-{Guid.NewGuid():N}", "hash", "عنوان", "0780")
        {
            CreationStep = UserCreationStep.Completed
        };
        user.UserBranches.Add(new UserBranch(branchId));
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserPermissions.Add(new UserPermission(user.Id, appRead.Id) { Permission = appRead });
        db.SalesEmployeeAssignments.Add(new SalesEmployeeAssignment(user.Id, branchId, T0, createdByUserId: 1));
        await db.SaveChangesAsync();
        return user;
    }

    private static CurrentUserServiceStub User(int userId, params string[] permissions)
        => new()
        {
            UserId = userId,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("permissions", string.Join(",", permissions))], "test"))
        };

    private static CurrentUserServiceStub Admin(int userId, params string[] permissions)
        => User(userId, permissions);

    private static TestApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }

    private sealed class ClockStub : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => T0.AddHours(2);
    }
}
