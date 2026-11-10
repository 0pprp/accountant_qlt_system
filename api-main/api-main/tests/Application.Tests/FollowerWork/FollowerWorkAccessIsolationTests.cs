using Application.FollowerWork.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Tests.FollowerWork;

/// <summary>
/// صلاحية قسم واحد لا تفتح بيانات الأقسام الأخرى؛ الدليل الأب لا يمنح عدادات الأقسام.
/// </summary>
public class FollowerWorkAccessIsolationTests
{
    [Fact]
    public void Section_permissions_are_distinct_from_parent()
    {
        Assert.NotEqual(SeedPermissions.FollowerWork.Read, SeedPermissions.FollowerWork.SalesReviewRead);
        Assert.All(
            FollowerWorkAccess.SectionDirectoryPermissions,
            p => Assert.NotEqual(SeedPermissions.FollowerWork.Read, p));
    }

    [Fact]
    public void Parent_read_does_not_equal_any_section_permission_name()
    {
        var sections = new[]
        {
            SeedPermissions.FollowerWork.SalesReviewRead,
            SeedPermissions.FollowerWork.DecliningRead,
            SeedPermissions.FollowerWork.NotesRead,
            SeedPermissions.FollowerWork.OldCustomersRead,
            SeedPermissions.FollowerWork.DelegateNotesRead,
            SeedPermissions.FollowerWork.CollectionRead,
            SeedPermissions.FollowerWork.LocationRead,
            SeedPermissions.FollowerWork.RouteRead,
        };
        Assert.All(sections, s => Assert.EndsWith(".Read", s));
        Assert.DoesNotContain(SeedPermissions.FollowerWork.Read, sections);
    }

    [Fact]
    public void Unavailable_metrics_are_not_zero_placeholders()
    {
        var dto = new FollowerWorkCardMetricsDto(
            null, null, null, null, null, null, null, "unavailable");
        Assert.Null(dto.PhantomRating);
        Assert.Null(dto.RatedCustomerCount);
        Assert.Equal("unavailable", dto.Availability);
    }
}
