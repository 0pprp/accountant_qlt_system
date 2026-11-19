using System.Security.Claims;
using Application.ManagementTasks.Common;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ManagementTasks.Queries.Admin.Assignees;
using Application.ManagementTasks.Queries.Admin.List;
using Domain.Entities.BranchAggregate;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Tests.ManagementTasks;

public class ManagementTaskAssigneeAudienceTests
{
    [Fact]
    public async Task Without_audience_returns_all_completed_branch_users()
    {
        await using var db = CreateDb();
        var ids = await SeedAsync(db);

        var result = await Handler(db).Handle(new GetManagementTaskAssigneeOptionsQuery { BranchId = 1 }, default);

        Assert.True(result.IsSucceeded);
        var userIds = result.Data!.Select(x => x.UserId).OrderBy(x => x).ToList();
        Assert.Equal(new[] { ids.Manager, ids.Plain, ids.Accountant }.OrderBy(x => x).ToList(), userIds);
    }

    [Fact]
    public async Task BranchManager_audience_returns_only_branch_manager_role_users_in_scope()
    {
        await using var db = CreateDb();
        var ids = await SeedAsync(db);

        var result = await Handler(db).Handle(
            new GetManagementTaskAssigneeOptionsQuery { BranchId = 1, Audience = "branchManager" },
            default);

        Assert.True(result.IsSucceeded);
        var option = Assert.Single(result.Data!);
        Assert.Equal(ids.Manager, option.UserId);
        Assert.Equal([1], option.BranchIds);
    }

    [Fact]
    public async Task BranchManager_audience_excludes_incomplete_and_other_branch_managers()
    {
        await using var db = CreateDb();
        var ids = await SeedAsync(db);

        var result = await Handler(db).Handle(
            new GetManagementTaskAssigneeOptionsQuery { Audience = "BRANCHMANAGER" },
            default);

        Assert.True(result.IsSucceeded);
        Assert.DoesNotContain(result.Data!, x => x.UserId == ids.IncompleteManager);
        Assert.DoesNotContain(result.Data!, x => x.UserId == ids.OtherBranchManager);
        Assert.Contains(result.Data!, x => x.UserId == ids.Manager);
    }

    [Fact]
    public async Task Unknown_audience_is_rejected()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var result = await Handler(db).Handle(
            new GetManagementTaskAssigneeOptionsQuery { BranchId = 1, Audience = "everyone" },
            default);

        Assert.True(result.IsFailed);
        Assert.Equal(ManagementTaskErrors.InvalidAudience.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Branch_outside_scope_is_forbidden_even_with_audience()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var result = await Handler(db).Handle(
            new GetManagementTaskAssigneeOptionsQuery { BranchId = 2, Audience = "branchManager" },
            default);

        Assert.True(result.IsFailed);
        Assert.Equal(ManagementTaskErrors.BranchForbidden.Code, result.Error!.Value.Code);
    }

    [Fact]
    public async Task Transfer_permission_alone_can_list_branch_managers()
    {
        await using var db = CreateDb();
        var ids = await SeedAsync(db);

        var result = await Handler(db, SeedPermissions.ManagementTask.Transfer).Handle(
            new GetManagementTaskAssigneeOptionsQuery { BranchId = 1, Audience = "branchManager" },
            default);

        Assert.True(result.IsSucceeded);
        Assert.Equal(ids.Manager, Assert.Single(result.Data!).UserId);
    }

    [Fact]
    public void ListItem_maps_every_lifecycle_stamp()
    {
        var created = new DateTimeOffset(2026, 11, 1, 6, 0, 0, TimeSpan.Zero);
        var transferredAt = created.AddHours(1);
        var branchAcceptedAt = created.AddHours(2);
        var branchCompletedAt = created.AddHours(3);
        var confirmedAt = created.AddHours(4);
        var task = NewTask();

        Assert.True(task.TryAccept(10, created.AddMinutes(30), out _));
        Assert.True(task.TryTransfer(10, 20, "مدير الفرع", null, transferredAt, out _));
        Assert.True(task.TryAccept(20, branchAcceptedAt, out _));
        Assert.True(task.TryComplete(20, branchCompletedAt, out _));
        Assert.True(task.TryConfirm(10, confirmedAt, out _));

        var dto = ManagementTaskMapper.ToListItem(task, confirmedAt);

        Assert.Equal(transferredAt, dto.TransferredAtUtc);
        Assert.Equal(branchCompletedAt, dto.BranchCompletedAtUtc);
        Assert.Equal(confirmedAt, dto.ConfirmedAtUtc);
        Assert.Equal(confirmedAt, dto.StatusChangedAtUtc);
        Assert.Null(dto.AssigneeCompletedAtUtc);
        Assert.Equal(ManagementTaskStatus.Completed, dto.Status);
    }

    [Fact]
    public void ListItem_maps_assignee_self_completion_stamp()
    {
        var at = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);
        var task = NewTask();

        Assert.True(task.TryAccept(10, at, out _));
        Assert.True(task.TryComplete(10, at.AddHours(1), out _));

        var dto = ManagementTaskMapper.ToListItem(task, at.AddHours(1));

        Assert.Equal(at.AddHours(1), dto.AssigneeCompletedAtUtc);
        Assert.Null(dto.TransferredAtUtc);
        Assert.Null(dto.ConfirmedAtUtc);
    }

    [Fact]
    public async Task Inbox_hides_task_held_by_branch_after_transfer_by_default()
    {
        await using var db = CreateDb();
        await SeedTransferredTaskAsync(db);

        var result = await ListHandler(db).Handle(Inbox(includeTransferred: false), default);

        Assert.True(result.IsSucceeded);
        Assert.Empty(result.Data!.Items);
    }

    [Fact]
    public async Task Inbox_with_transferred_flag_keeps_task_visible_to_assignee()
    {
        await using var db = CreateDb();
        var taskId = await SeedTransferredTaskAsync(db);

        var result = await ListHandler(db).Handle(Inbox(includeTransferred: true), default);

        Assert.True(result.IsSucceeded);
        var item = Assert.Single(result.Data!.Items);
        Assert.Equal(taskId, item.Id);
        Assert.Equal(ManagementTaskStatus.Accepted, item.Status);
        Assert.Equal(20, item.CurrentHolderUserId);
        Assert.NotNull(item.TransferredAtUtc);
    }

    [Fact]
    public async Task Inbox_with_transferred_flag_ignores_tasks_of_other_assignees()
    {
        await using var db = CreateDb();
        await SeedTransferredTaskAsync(db, assigneeId: 11);

        var result = await ListHandler(db).Handle(Inbox(includeTransferred: true), default);

        Assert.True(result.IsSucceeded);
        Assert.Empty(result.Data!.Items);
    }

    private static GetManagementTasksQuery Inbox(bool includeTransferred)
        => new()
        {
            Pagination = new Pagination(1, 50),
            InboxForMe = true,
            IncludeTransferredByMe = includeTransferred
        };

    private static async Task<int> SeedTransferredTaskAsync(TestApplicationDbContext db, int assigneeId = 10)
    {
        db.Branches.Add(new Branch("فرع أ", 10));
        db.Users.Add(new User("المدير", null, null, null, $"user-{Guid.NewGuid():N}", "hash", null, null));
        await db.SaveChangesAsync();
        var at = new DateTimeOffset(2026, 11, 3, 8, 0, 0, TimeSpan.Zero);
        var task = new ManagementTask(
            "مهمة",
            "وصف",
            ManagementTaskPriority.Medium,
            new DateOnly(2026, 11, 30),
            null,
            1,
            1,
            assigneeId,
            "المفوض",
            null,
            null);
        Assert.True(task.TryAccept(assigneeId, at, out _));
        Assert.True(task.TryTransfer(assigneeId, 20, "مدير الفرع", null, at.AddHours(1), out _));
        Assert.True(task.TryAccept(20, at.AddHours(2), out _));
        db.ManagementTasks.Add(task);
        await db.SaveChangesAsync();
        return task.Id;
    }

    private static GetManagementTasksHandler ListHandler(TestApplicationDbContext db)
        => new(db, new CurrentUserServiceStub
        {
            UserId = 10,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", SeedPermissions.ManagementTask.Accept)], "test"))
        }, new FixedClock());

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNowOffset => new(2026, 11, 4, 9, 0, 0, TimeSpan.Zero);
    }

    private static ManagementTask NewTask()
        => new(
            "عنوان",
            "وصف",
            ManagementTaskPriority.High,
            new DateOnly(2026, 11, 30),
            null,
            1,
            1,
            10,
            "المفوض",
            null,
            null);

    private sealed record SeededIds(int Manager, int Plain, int Accountant, int IncompleteManager, int OtherBranchManager);

    private static async Task<SeededIds> SeedAsync(TestApplicationDbContext db)
    {
        db.Branches.AddRange(new Branch("فرع أ", 10), new Branch("فرع ب", 20));
        var managerRole = new Role(SeedRoles.BranchManager.Name, SeedRoles.BranchManager.DisplayName);
        var accountantRole = new Role(SeedRoles.BranchAccountant.Name, SeedRoles.BranchAccountant.DisplayName);
        db.Roles.AddRange(managerRole, accountantRole);
        await db.SaveChangesAsync();

        var manager = await AddUserAsync(db, "مدير فرع أ", 1, managerRole.Id, UserCreationStep.Completed);
        var plain = await AddUserAsync(db, "مستخدم عادي", 1, null, UserCreationStep.Completed);
        var accountant = await AddUserAsync(db, "محاسب فرع", 1, accountantRole.Id, UserCreationStep.Completed);
        var incomplete = await AddUserAsync(db, "مدير غير مكتمل", 1, managerRole.Id, UserCreationStep.Permissions);
        var other = await AddUserAsync(db, "مدير فرع ب", 2, managerRole.Id, UserCreationStep.Completed);

        return new SeededIds(manager, plain, accountant, incomplete, other);
    }

    private static async Task<int> AddUserAsync(
        TestApplicationDbContext db,
        string name,
        int branchId,
        int? roleId,
        UserCreationStep step)
    {
        var user = new User(name, null, null, null, $"user-{Guid.NewGuid():N}", "hash", null, null)
        {
            CreationStep = step
        };
        user.UserBranches.Add(new UserBranch(branchId));
        db.Users.Add(user);
        await db.SaveChangesAsync();
        if (roleId is int id)
        {
            db.UserRoles.Add(new UserRole(user.Id, id));
            await db.SaveChangesAsync();
        }

        return user.Id;
    }

    private static GetManagementTaskAssigneeOptionsHandler Handler(
        TestApplicationDbContext db,
        string permission = SeedPermissions.ManagementTask.Assign)
        => new(db, new CurrentUserServiceStub
        {
            UserId = 10,
            BranchIds = [1],
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permissions", permission)], "test"))
        });

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
