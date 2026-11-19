using Application.DelegateFollowup.Common;
using Application.DelegateFollowup.Queries.Admin.GetSalesOrders;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderListAggregate;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.DelegateFollowup;

public class DelegateFollowupSalesOrdersAttributionTests
{
    private const int BranchId = 1;
    private const int SharedDelegate = 100;
    private const int OtherDelegate = 200;

    [Fact]
    public async Task EachDelegateRequest_IsCountedInExactlyOneList()
    {
        await using var db = CreateDb();
        var listA = new OrderList("أ", SharedDelegate, 9, BranchId);
        var listB = new OrderList("ب", SharedDelegate, 9, BranchId);
        var listC = new OrderList("ج", OtherDelegate, 9, BranchId);
        db.OrderLists.AddRange(listA, listB, listC);

        var customer = new Customer("زبون", "ام", "1", new DateOnly(1990, 1, 1), "07700000001", "07700000001",
            new Business("محل", "عنوان", "علامة"), branchId: BranchId);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var orderOnA = NewOrder(customer.Id, listA.Id, new DateOnly(2026, 8, 1));
        var otherOrderOnA = NewOrder(customer.Id, listA.Id, new DateOnly(2026, 6, 1));
        var recentOrderOnB = NewOrder(customer.Id, listB.Id, new DateOnly(2026, 8, 10));
        db.Orders.AddRange(orderOnA, otherOrderOnA, recentOrderOnB);
        await db.SaveChangesAsync();

        var unlinked = Request(SharedDelegate, new DateTimeOffset(2026, 8, 15, 9, 0, 0, TimeSpan.Zero));
        var linkedA = Request(SharedDelegate, new DateTimeOffset(2026, 8, 2, 9, 0, 0, TimeSpan.Zero));
        linkedA.LinkedOrderId = orderOnA.Id;
        linkedA.Status = SalesRequestStatus.Completed;
        var otherDelegateSoldIntoA = Request(OtherDelegate, new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero));
        otherDelegateSoldIntoA.LinkedOrderId = otherOrderOnA.Id;
        otherDelegateSoldIntoA.Status = SalesRequestStatus.Completed;
        var followerRequest = new SalesRequest("متابع", null, SalesRequestSourceType.Follower, BranchId, null, null, SharedDelegate);
        db.SalesRequests.AddRange(unlinked, linkedA, otherDelegateSoldIntoA, followerRequest);
        await db.SaveChangesAsync();

        var a = await Load(db, listA.Id);
        var b = await Load(db, listB.Id);
        var c = await Load(db, listC.Id);

        Assert.Equal(new[] { linkedA.Id, otherDelegateSoldIntoA.Id }.OrderBy(x => x), a.Orders.Select(o => o.RequestId).OrderBy(x => x));
        Assert.Equal([unlinked.Id], b.Orders.Select(o => o.RequestId));
        Assert.Empty(c.Orders);

        var all = a.Orders.Concat(b.Orders).Concat(c.Orders).Select(o => o.RequestId).ToList();
        Assert.Equal(all.Count, all.Distinct().Count());
        Assert.DoesNotContain(followerRequest.Id, all);
        Assert.Equal(2, a.Sold);
        Assert.Equal(1, b.Open);
    }

    private static async Task<DelegateSalesOrdersDto> Load(TestApplicationDbContext db, int listId)
    {
        var handler = new GetDelegateFollowupSalesOrdersHandler(
            db,
            new CurrentUserServiceStub { BranchIds = [BranchId] },
            new DateTimeProviderStub());
        var result = await handler.Handle(new GetDelegateFollowupSalesOrdersQuery(listId, BranchId), CancellationToken.None);
        Assert.True(result.IsSucceeded);
        return result.Data!;
    }

    private static Order NewOrder(int customerId, int listId, DateOnly saleDate)
    {
        var order = Order.CreateByAdmin(600_000, 1_000_000, 0, 10_000, customerId, branchId: BranchId, orderItems: []);
        order.OrderListId = listId;
        order.SaleDate = saleDate;
        return order;
    }

    private static SalesRequest Request(int creator, DateTimeOffset createdAt)
        => new("طلب", "07711111111", SalesRequestSourceType.Delegate, BranchId, null, null, creator)
        {
            CreatedAt = createdAt
        };

    private static TestApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
