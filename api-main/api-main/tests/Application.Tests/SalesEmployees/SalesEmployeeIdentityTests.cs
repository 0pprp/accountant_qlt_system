using Application.SalesEmployees.Common;

namespace Application.Tests.SalesEmployees;

public class SalesEmployeeIdentityTests
{
    [Theory]
    [InlineData("Active", true)]
    [InlineData("NoShift", true)]
    [InlineData("Offline", false)]
    [InlineData("", false)]
    public void AllowedShiftStates_whitelist(string state, bool expected) =>
        Assert.Equal(expected, SalesEmployeeIdentity.AllowedShiftStates.Contains(state));

    [Fact]
    public void VisibleBranches_intersects_viewer_scope_only()
    {
        var visible = SalesEmployeeIdentity.VisibleBranches([1, 2, 9], [1, 9, 5]);
        Assert.Equal(new[] { 1, 9 }, visible);
    }

    [Theory]
    [InlineData("name", true)]
    [InlineData("rating", true)]
    [InlineData("salary", false)]
    public void AllowedSorts_whitelist(string sort, bool expected) =>
        Assert.Equal(expected, SalesEmployeeIdentity.AllowedSorts.Contains(sort));
}
