using Domain.Entities.SalesLocationAggregate.Enums;

namespace Application.Tests.SalesEmployeeTracking;

public class SalesLocationActorIsolationTests
{
    [Fact]
    public void Actor_kinds_are_distinct_for_sales_and_follower()
    {
        Assert.NotEqual(SalesLocationActorKind.SalesEmployee, SalesLocationActorKind.Follower);
        Assert.Equal(0, (byte)SalesLocationActorKind.Unknown);
    }
}
