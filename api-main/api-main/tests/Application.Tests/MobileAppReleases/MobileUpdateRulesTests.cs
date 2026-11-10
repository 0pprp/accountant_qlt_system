using Domain.Entities.MobileAppReleaseAggregate;

namespace Application.Tests.MobileAppReleases;

public class MobileUpdateRulesTests
{
    [Fact]
    public void None_when_current_at_or_above_latest()
    {
        Assert.Equal(
            MobileUpdateKind.None,
            MobileUpdateRules.Evaluate(5, 5, 3, forceUpdate: true));
    }

    [Fact]
    public void Mandatory_when_below_minimum()
    {
        Assert.Equal(
            MobileUpdateKind.Mandatory,
            MobileUpdateRules.Evaluate(2, 10, 5, forceUpdate: false));
    }

    [Fact]
    public void Mandatory_when_force_flag()
    {
        Assert.Equal(
            MobileUpdateKind.Mandatory,
            MobileUpdateRules.Evaluate(7, 10, 5, forceUpdate: true));
    }

    [Fact]
    public void Optional_when_outdated_but_supported()
    {
        Assert.Equal(
            MobileUpdateKind.Optional,
            MobileUpdateRules.Evaluate(7, 10, 5, forceUpdate: false));
    }
}
