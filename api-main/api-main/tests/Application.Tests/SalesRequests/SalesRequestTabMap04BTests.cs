using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;

namespace Application.Tests.SalesRequests;

public class SalesRequestTabMap04BTests
{
    [Fact]
    public void Workflow_tabs_cover_matrix_without_legal_or_deleted()
    {
        Assert.Contains(SalesRequestTabMap.Unassigned, SalesRequestTabMap.WorkflowTabs);
        Assert.Contains(SalesRequestTabMap.Incoming, SalesRequestTabMap.WorkflowTabs);
        Assert.Contains(SalesRequestTabMap.Sold, SalesRequestTabMap.WorkflowTabs);
        Assert.DoesNotContain("legal", SalesRequestTabMap.WorkflowTabs);
        Assert.DoesNotContain("deleted", SalesRequestTabMap.WorkflowTabs);
    }

    [Fact]
    public void Extra_views_split_between_implemented_and_not_implemented_in_04E()
    {
        Assert.Equal(
            ["sent", "exceptions", "deleted", "legal"],
            SalesRequestTabMap.ExtraViewsImplemented);
        Assert.Empty(SalesRequestTabMap.ExtraViewsNotImplemented);
        Assert.DoesNotContain("all", SalesRequestTabMap.ExtraViewsNotImplemented);
        Assert.DoesNotContain("all", SalesRequestTabMap.ExtraViewsImplemented);
    }

    [Fact]
    public void Prepared_includes_ConvertedToSale_not_Completed()
    {
        var prepared = SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.Prepared);
        Assert.Contains(SalesRequestStatus.ConvertedToSale, prepared);
        Assert.DoesNotContain(SalesRequestStatus.Completed, prepared);
        Assert.Equal(
            [SalesRequestStatus.Completed],
            SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.Sold));
    }

    [Fact]
    public void All_and_unassigned_have_no_status_list()
    {
        Assert.Empty(SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.All));
        Assert.Empty(SalesRequestTabMap.StatusesForTab(SalesRequestTabMap.Unassigned));
        Assert.True(SalesRequestTabMap.IsUnassignedTab("unassigned"));
        Assert.True(SalesRequestTabMap.IsAllTab("all"));
    }

    [Fact]
    public void Arabic_labels_for_workspace()
    {
        Assert.Equal("غير المسند", SalesRequestTabMap.ArabicLabel(SalesRequestTabMap.Unassigned));
        Assert.Equal("الكل", SalesRequestTabMap.ArabicLabel(SalesRequestTabMap.All));
        Assert.Equal("الاستثناءات", SalesRequestTabMap.ArabicLabel("exceptions"));
    }
}
