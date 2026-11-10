using Application.SalesEmployees.Common;
using Domain.Entities.SalesEmployeeAggregate;

namespace Application.Tests.SalesEmployees;

public class SalesWorkforceTests
{
    [Fact]
    public void Assignment_IsActiveAt_respects_window()
    {
        var start = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        var a = new SalesEmployeeAssignment(10, 1, start, 1);
        Assert.True(a.IsActiveAt(start.AddHours(1)));
        a.End(start.AddDays(1));
        Assert.False(a.IsActiveAt(start.AddDays(2)));
        Assert.True(a.IsActiveAt(start.AddHours(12)));
    }

    [Fact]
    public void VisibleBranches_does_not_leak_out_of_scope()
    {
        var visible = SalesWorkforce.VisibleBranches([1, 2, 9], [1, 5]);
        Assert.Equal(new[] { 1 }, visible);
    }
}
