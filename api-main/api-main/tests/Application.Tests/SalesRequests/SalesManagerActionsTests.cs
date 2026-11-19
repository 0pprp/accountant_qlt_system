using System.Security.Claims;
using Application.Common.Interfaces;
using Application.SalesRequests.Commands.Admin.Assign;
using Application.SalesRequests.Commands.Admin.AssignBatch;
using Application.SalesRequests.Commands.Admin.CancelToUnassigned;
using Application.SalesRequests.Commands.Admin.ManagerProgress;
using Application.SalesRequests.Commands.Admin.Return;
using Application.SalesRequests.Commands.Admin.TransferBranch;
using Application.SalesRequests.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.SalesEmployeeAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

public class SalesManagerActionsTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Return_sends_rejected_request_back_to_same_employee()
    {
        await using var db = CreateDb();
        var employee = await AddEligibleUserAsync(db, 1);
        var request = await SeedAsync(db, r =>
        {
            r.Assign(employee.Id, T0);
            r.Reject("الزبون غير موجود", T0);
        });

        var result = await new ReturnSalesRequestHandler(db, Manager(), new ClockStub())
            .Handle(new ReturnSalesRequestCommand { RequestId = request.Id, Note = "راجع العنوان" }, default);

        Assert.True(result.IsSucceeded);
        var saved = await db.SalesRequests.SingleAsync(x => x.Id == request.Id);
        Assert.Equal(SalesRequestStatus.Returned, saved.Status);
        Assert.Equal(employee.Id, saved.TargetEmployeeId);
        Assert.Null(saved.ViewedAtUtc);
    }

    [Fact]
    public async Task Return_requires_a_previous_employee_and_a_rejected_status()
    {
        await using var db = CreateDb();
        var orphan = await SeedAsync(db, r => r.Reject("رفض", T0, clearAssignee: true));
        var fresh = await SeedAsync(db, _ => { });
        var handler = new ReturnSalesRequestHandler(db, Manager(), new ClockStub());

        var noEmployee = await handler.Handle(new ReturnSalesRequestCommand { RequestId = orphan.Id, Note = "x" }, default);
        var notRejected = await handler.Handle(new ReturnSalesRequestCommand { RequestId = fresh.Id, Note = "x" }, default);

        Assert.Equal(SalesRequestErrors.ReturnRequiresEmployee.Code, noEmployee.Error!.Value.Code);
        Assert.Equal(SalesRequestErrors.IllegalTransition.Code, notRejected.Error!.Value.Code);
    }

    [Theory]
    [InlineData(ManagerProgressAction.Prepare, SalesRequestStatus.Inspected, true)]
    [InlineData(ManagerProgressAction.Prepare, SalesRequestStatus.Pending, true)]
    [InlineData(ManagerProgressAction.Prepare, SalesRequestStatus.Assigned, false)]
    [InlineData(ManagerProgressAction.Pend, SalesRequestStatus.Inspected, true)]
    [InlineData(ManagerProgressAction.Pend, SalesRequestStatus.PreparedForSale, true)]
    [InlineData(ManagerProgressAction.Pend, SalesRequestStatus.Completed, false)]
    public void Manager_progress_rules_match_source(ManagerProgressAction action, SalesRequestStatus from, bool allowed)
        => Assert.Equal(allowed, ManagerProgressRules.IsAllowed(action, from));

    [Fact]
    public async Task Manager_prepares_inspected_request_and_pend_needs_note()
    {
        await using var db = CreateDb();
        var request = await SeedAsync(db, r =>
        {
            r.Assign(7, T0);
            r.Inspect(T0);
        });
        var handler = new ManagerProgressSalesRequestHandler(db, Manager(), new ClockStub());

        var prepared = await handler.Handle(new ManagerProgressSalesRequestCommand
        {
            RequestId = request.Id,
            Action = ManagerProgressAction.Prepare
        }, default);

        Assert.True(prepared.IsSucceeded);
        Assert.Equal(SalesRequestStatus.PreparedForSale, (await db.SalesRequests.SingleAsync()).Status);
        Assert.False(new ManagerProgressSalesRequestCommandValidator()
            .Validate(new ManagerProgressSalesRequestCommand { RequestId = 1, Action = ManagerProgressAction.Pend })
            .IsValid);
    }

    [Fact]
    public async Task Cancel_returns_request_to_unassigned_and_cancels_open_legal_case()
    {
        await using var db = CreateDb();
        var request = await SeedAsync(db, r => r.SendToLegalFollowup("قانونية", T0));
        db.LegalFollowupCases.Add(new LegalFollowupCase(
            request.Id, null, 9, "متابع", request.CustomerName, request.CustomerPhone, "عنوان", null, 1, "مدير", T0));
        await db.SaveChangesAsync();

        var result = await new CancelSalesRequestToUnassignedHandler(db, Manager(), new ClockStub())
            .Handle(new CancelSalesRequestToUnassignedCommand { RequestId = request.Id, Reason = "أرسلت بالخطأ" }, default);

        Assert.True(result.IsSucceeded);
        var saved = await db.SalesRequests.SingleAsync();
        Assert.Equal(SalesRequestStatus.New, saved.Status);
        Assert.Null(saved.TargetEmployeeId);
        Assert.True(saved.IsUnassigned());
        Assert.Equal(LegalFollowupStatus.Cancelled, (await db.LegalFollowupCases.SingleAsync()).Status);
    }

    [Theory]
    [InlineData(SalesRequestStatus.Completed)]
    [InlineData(SalesRequestStatus.Rejected)]
    [InlineData(SalesRequestStatus.New)]
    public async Task Cancel_is_refused_for_sold_rejected_and_already_unassigned(SalesRequestStatus status)
    {
        await using var db = CreateDb();
        var request = await SeedAsync(db, r => r.Status = status);

        var result = await new CancelSalesRequestToUnassignedHandler(db, Manager(), new ClockStub())
            .Handle(new CancelSalesRequestToUnassignedCommand { RequestId = request.Id, Reason = "x" }, default);

        Assert.Equal(SalesRequestErrors.CancelNotAllowed.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Transfer_moves_request_to_target_branch_as_unassigned()
    {
        await using var db = CreateDb();
        await SeedBranchesAsync(db);
        var request = await SeedAsync(db, r => r.Assign(7, T0));

        var result = await new TransferSalesRequestBranchHandler(db, Manager(branches: [1, 2]), new ClockStub())
            .Handle(new TransferSalesRequestBranchCommand { RequestId = request.Id, TargetBranchId = 2, Reason = "الزبون انتقل" }, default);

        Assert.True(result.IsSucceeded);
        var saved = await db.SalesRequests.SingleAsync();
        Assert.Equal(2, saved.BranchId);
        Assert.Equal(20, saved.ProvinceId);
        Assert.Equal(SalesRequestStatus.New, saved.Status);
        Assert.Null(saved.TargetEmployeeId);
        Assert.Contains(await db.SalesRequestTimelineEntries.ToListAsync(),
            e => e.EventType == SalesRequestTimelineEventType.BranchTransferred);
    }

    [Fact]
    public async Task Transfer_is_refused_outside_scope_same_branch_and_for_held_or_legal_requests()
    {
        await using var db = CreateDb();
        await SeedBranchesAsync(db);
        var held = await SeedAsync(db, r => r.HoldForException(SalesRequestExceptionHoldStatus.Pending));
        var legal = await SeedAsync(db, r => r.SendToLegalFollowup(null, T0));
        var normal = await SeedAsync(db, _ => { });
        var handler = new TransferSalesRequestBranchHandler(db, Manager(branches: [1, 2]), new ClockStub());
        var narrow = new TransferSalesRequestBranchHandler(db, Manager(branches: [1]), new ClockStub());

        Assert.Equal(SalesRequestErrors.TransferNotAllowed.Code,
            (await handler.Handle(Transfer(held.Id, 2), default)).Error!.Value.Code);
        Assert.Equal(SalesRequestErrors.TransferNotAllowed.Code,
            (await handler.Handle(Transfer(legal.Id, 2), default)).Error!.Value.Code);
        Assert.Equal(SalesRequestErrors.TransferSameBranch.Code,
            (await handler.Handle(Transfer(normal.Id, 1), default)).Error!.Value.Code);
        Assert.Equal(SalesRequestErrors.InvalidBranch.Code,
            (await narrow.Handle(Transfer(normal.Id, 2), default)).Error!.Value.Code);
    }

    [Fact]
    public async Task Batch_assign_assigns_unassigned_and_reports_the_rest()
    {
        await using var db = CreateDb();
        var employee = await AddEligibleUserAsync(db, 1);
        var a = await SeedAsync(db, _ => { });
        var b = await SeedAsync(db, _ => { });
        var busy = await SeedAsync(db, r => r.Assign(employee.Id, T0));
        var user = Manager();
        var sender = new AssignOnlySender(db, user);

        var result = await new AssignSalesRequestBatchHandler(db, user, sender)
            .Handle(new AssignSalesRequestBatchCommand { RequestIds = [a.Id, b.Id, busy.Id], EmployeeId = employee.Id }, default);

        Assert.True(result.IsSucceeded);
        Assert.Equal(2, result.Data!.Succeeded);
        Assert.Equal(1, result.Data.Failed);
        Assert.Equal(SalesRequestErrors.NotUnassigned.Code, result.Data.Items.Single(i => i.RequestId == busy.Id).ErrorCode);
        Assert.All(await db.SalesRequests.Where(x => x.Id != busy.Id).ToListAsync(),
            r => Assert.Equal(SalesRequestStatus.Assigned, r.Status));
    }

    [Fact]
    public async Task Rejected_exception_moves_request_to_rejected_and_blocks_assignment()
    {
        await using var db = CreateDb();
        var employee = await AddEligibleUserAsync(db, 1);
        var request = await SeedAsync(db, _ => { });
        var requested = await new Application.SalesRequests.Commands.Admin.RequestException.RequestSalesRequestExceptionHandler(
                db, UserWith(5, SeedPermissions.SalesRequest.RequestException), new ClockStub())
            .Handle(new Application.SalesRequests.Commands.Admin.RequestException.RequestSalesRequestExceptionCommand
            {
                RequestId = request.Id,
                Reason = "زبون عليه دين"
            }, default);
        Assert.True(requested.IsSucceeded);

        var rejected = await new Application.SalesRequests.Commands.Admin.RejectException.RejectSalesRequestExceptionHandler(
                db, UserWith(6, SeedPermissions.SalesRequest.ExceptionDecide), new ClockStub())
            .Handle(new Application.SalesRequests.Commands.Admin.RejectException.RejectSalesRequestExceptionCommand
            {
                RequestId = request.Id,
                Reason = "لا يُباع"
            }, default);

        Assert.True(rejected.IsSucceeded);
        var saved = await db.SalesRequests.SingleAsync();
        Assert.Equal(SalesRequestStatus.Rejected, saved.Status);
        Assert.Equal("rejected", SalesRequestAdminQuery.TabKeyFor(saved.Status, saved.TargetEmployeeId, saved.ExceptionHoldStatus, saved.ExceptionConsumedAtUtc));
        var assign = await new AssignSalesRequestHandler(db, Manager(), new ClockStub())
            .Handle(new AssignSalesRequestCommand { RequestId = request.Id, EmployeeId = employee.Id }, default);
        Assert.True(assign.IsFailed);
    }

    private static CurrentUserServiceStub UserWith(int userId, string permission)
        => new()
        {
            UserId = userId,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", permission)], "test"))
        };

    [Fact]
    public void Batch_validator_caps_at_fifty_and_rejects_duplicates()
    {
        var validator = new AssignSalesRequestBatchCommandValidator();
        Assert.False(validator.Validate(new AssignSalesRequestBatchCommand
        {
            RequestIds = Enumerable.Range(1, 51).ToList(),
            EmployeeId = 1
        }).IsValid);
        Assert.False(validator.Validate(new AssignSalesRequestBatchCommand { RequestIds = [1, 1], EmployeeId = 1 }).IsValid);
        Assert.True(validator.Validate(new AssignSalesRequestBatchCommand { RequestIds = [1, 2], EmployeeId = 1 }).IsValid);
    }

    private static TransferSalesRequestBranchCommand Transfer(int id, int branchId)
        => new() { RequestId = id, TargetBranchId = branchId, Reason = "نقل" };

    private static async Task SeedBranchesAsync(TestApplicationDbContext db)
    {
        db.Branches.AddRange(new Branch("فرع أ", 10), new Branch("فرع ب", 20));
        await db.SaveChangesAsync();
    }

    private static async Task<SalesRequest> SeedAsync(TestApplicationDbContext db, Action<SalesRequest> arrange)
    {
        var request = new SalesRequest("علي حسن كاظم", "07801112233", SalesRequestSourceType.Manual, 1, null, null, null);
        arrange(request);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    private static async Task<User> AddEligibleUserAsync(TestApplicationDbContext db, int branchId)
    {
        var appRead = await db.Permissions.FirstOrDefaultAsync(p => p.Name == SeedPermissions.SalesRequest.AppRead);
        if (appRead is null)
        {
            appRead = new Permission(SeedPermissions.SalesRequest.AppRead, "قراءة تطبيق طلبات البيع", "SalesRequest", "طلبات البيع");
            db.Permissions.Add(appRead);
            await db.SaveChangesAsync();
        }

        var user = new User("موظف", "أم", "123", new DateOnly(1990, 1, 1), $"user-{Guid.NewGuid():N}", "hash", "عنوان", "0780")
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

    private static CurrentUserServiceStub Manager(List<int>? branches = null)
        => new()
        {
            UserId = 4,
            BranchIds = branches ?? [1],
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", SeedPermissions.SalesRequest.Assign)], "test"))
        };

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class ClockStub : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => T0.AddHours(2);
    }

    private sealed class AssignOnlySender(TestApplicationDbContext db, ICurrentUserService user) : ISender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is AssignSalesRequestCommand assign)
            {
                object result = await new AssignSalesRequestHandler(db, user, new ClockStub()).Handle(assign, cancellationToken);
                return (TResponse)result;
            }

            throw new NotSupportedException();
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
