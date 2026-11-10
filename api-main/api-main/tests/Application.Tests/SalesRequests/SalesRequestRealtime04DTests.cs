using System.Text.Json;
using Application.Common.Interfaces;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.SalesRequests;

public class SalesRequestRealtime04DTests
{
    private static readonly string[] ForbiddenFragments =
        ["name", "phone", "amount", "price", "money", "reason", "note", "customer"];

    [Fact]
    public void Outbox_entity_has_no_pii_or_money_properties()
    {
        var names = typeof(SalesRequestChangeOutbox).GetProperties().Select(p => p.Name.ToLowerInvariant());

        foreach (var name in names)
            Assert.DoesNotContain(ForbiddenFragments, f => name.Contains(f));
    }

    [Fact]
    public void Payload_shape_is_ids_and_metadata_only()
    {
        var row = new SalesRequestChangeOutbox(
            Guid.NewGuid(), 42, 3, DateTimeOffset.UtcNow, SalesRequestChangeType.Reassigned, 7, 11, 9);

        var json = JsonSerializer.Serialize(SalesRequestChangedPayload.From(row), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var doc = JsonDocument.Parse(json);
        var keys = doc.RootElement.EnumerateObject().Select(p => p.Name).OrderBy(x => x).ToArray();

        var expected = new[]
        {
            "audience", "branchId", "changeType", "eventId", "occurredAtUtc",
            "previousTargetEmployeeId", "requestId", "revision", "targetEmployeeId"
        }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, keys);
        Assert.Equal("Reassigned", doc.RootElement.GetProperty("changeType").GetString());
        Assert.Equal(3, doc.RootElement.GetProperty("revision").GetInt32());
        foreach (var key in keys)
            Assert.DoesNotContain(ForbiddenFragments, f => key.ToLowerInvariant().Contains(f));
    }

    [Fact]
    public void Group_names_follow_convention()
    {
        Assert.Equal("sr:user:5", SalesRequestHubGroups.User(5));
        Assert.Equal("sr:branch-admin:2", SalesRequestHubGroups.BranchAdmin(2));
    }

    [Fact]
    public void ForChange_targets_branch_new_and_previous_assignee()
    {
        var groups = SalesRequestHubGroups.ForChange(2, 11, 9);
        Assert.Equal(["sr:branch-admin:2", "sr:user:11", "sr:user:9"], groups);

        var unassigned = SalesRequestHubGroups.ForChange(2, null, null);
        Assert.Equal(["sr:branch-admin:2"], unassigned);
    }

    [Fact]
    public void ForIdentity_without_permissions_has_no_groups()
    {
        var identity = new SalesRequestHubIdentity(5, [1, 2], new HashSet<string>());
        Assert.Empty(SalesRequestHubGroups.ForIdentity(identity));
    }

    [Fact]
    public void ForIdentity_requires_tab_read_for_branch_admin_groups()
    {
        var identity = new SalesRequestHubIdentity(
            5, [1, 2], new HashSet<string> { SeedPermissions.SalesRequest.TabIncoming });

        var groups = SalesRequestHubGroups.ForIdentity(identity);

        Assert.Contains("sr:branch-admin:1", groups);
        Assert.Contains("sr:branch-admin:2", groups);
        Assert.DoesNotContain("sr:user:5", groups);
    }

    [Fact]
    public void ForIdentity_employee_gets_only_own_user_group()
    {
        var identity = new SalesRequestHubIdentity(
            5, [1], new HashSet<string> { SeedPermissions.SalesRequest.AppRead });

        var groups = SalesRequestHubGroups.ForIdentity(identity);

        Assert.Equal(["sr:user:5"], groups);
    }

    [Fact]
    public async Task Writer_persists_outbox_in_same_save_as_request_change()
    {
        await using var db = CreateDb();
        var request = new SalesRequest("اسم زبون", "07801112233", SalesRequestSourceType.Manual, 3, null, null, null);
        db.SalesRequests.Add(request);
        await db.SaveChangesAsync();

        request.Assign(9, DateTimeOffset.UtcNow);
        SalesRequestChangeOutboxWriter.Append(db, request, SalesRequestChangeType.Assigned, DateTimeOffset.UtcNow, null);
        await db.SaveChangesAsync();

        var row = await db.SalesRequestChangeOutboxes.SingleAsync();
        Assert.Equal(request.Id, row.RequestId);
        Assert.Equal(request.ConcurrencyStamp, row.Revision);
        Assert.Equal(3, row.BranchId);
        Assert.Equal(9, row.TargetEmployeeId);
        Assert.Null(row.PublishedAtUtc);
        Assert.NotEqual(Guid.Empty, row.EventId);
    }

    [Fact]
    public async Task Publish_failure_leaves_row_unpublished_for_retry()
    {
        await using var db = CreateDb();
        SalesRequestChangeOutboxWriter.Append(db, new SalesRequestChangeMessage(
            1, 1, SalesRequestChangeType.Assigned, 3, 9, null, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var publisher = new RecordingPublisher { Fail = true };
        var processor = CreateProcessor(db, publisher, new ClockStub());

        var published = await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Equal(0, published);
        var row = await db.SalesRequestChangeOutboxes.SingleAsync();
        Assert.Null(row.PublishedAtUtc);
        Assert.Equal(1, row.RetryCount);
        Assert.NotNull(row.LastAttemptAtUtc);
    }

    [Fact]
    public async Task Failed_row_is_retried_after_delay_and_then_published()
    {
        await using var db = CreateDb();
        SalesRequestChangeOutboxWriter.Append(db, new SalesRequestChangeMessage(
            1, 1, SalesRequestChangeType.Assigned, 3, 9, null, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var clock = new ClockStub();
        var publisher = new RecordingPublisher { Fail = true };
        await CreateProcessor(db, publisher, clock).ProcessBatchAsync(CancellationToken.None);

        publisher.Fail = false;
        Assert.Equal(0, await CreateProcessor(db, publisher, clock).ProcessBatchAsync(CancellationToken.None));

        clock.Now = clock.Now.Add(SalesRequestOutboxProcessor.RetryDelay).AddSeconds(1);
        Assert.Equal(1, await CreateProcessor(db, publisher, clock).ProcessBatchAsync(CancellationToken.None));

        var row = await db.SalesRequestChangeOutboxes.SingleAsync();
        Assert.NotNull(row.PublishedAtUtc);
        Assert.Single(publisher.Published);
        Assert.Equal(["sr:branch-admin:3", "sr:user:9"], publisher.Published[0].Groups);
    }

    private static TestApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }

    private static SalesRequestOutboxProcessor CreateProcessor(
        IApplicationDbContext db, ISalesRequestRealtimePublisher publisher, IDateTimeProvider clock)
        => new(db, publisher, clock, NullLogger<SalesRequestOutboxProcessor>.Instance);

    private sealed class ClockStub : IDateTimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);
        public DateTimeOffset UtcNowOffset => Now;
    }

    private sealed class RecordingPublisher : ISalesRequestRealtimePublisher
    {
        public bool Fail { get; set; }
        public List<(SalesRequestChangedPayload Payload, List<string> Groups)> Published { get; } = [];

        public Task PublishAsync(
            SalesRequestChangedPayload payload,
            IReadOnlyCollection<string> groups,
            CancellationToken cancellationToken)
        {
            if (Fail)
                throw new InvalidOperationException("hub down");

            Published.Add((payload, groups.ToList()));
            return Task.CompletedTask;
        }
    }
}
