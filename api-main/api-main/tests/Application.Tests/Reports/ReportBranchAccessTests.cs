using Application.Reports.Common;
using Application.Tests;
using Xunit;

namespace Application.Tests.Reports;

public class ReportBranchAccessTests
{
    [Fact]
    public void DenyIfOutOfScope_AllowsAdminForAnyBranch()
    {
        var user = new CurrentUserServiceStub
        {
            Role = Application.Common.SeedData.Roles.Admin.Name,
            BranchIds = []
        };

        var error = ReportBranchAccess.DenyIfOutOfScope(user, branchId: 99);

        Assert.Null(error);
    }

    [Fact]
    public void DenyIfOutOfScope_AllowsUserWhenBranchAssigned()
    {
        var user = new CurrentUserServiceStub
        {
            Role = "BranchAccountant",
            BranchIds = [1, 5]
        };

        var error = ReportBranchAccess.DenyIfOutOfScope(user, branchId: 5);

        Assert.Null(error);
    }

    [Fact]
    public void DenyIfOutOfScope_RejectsUserWhenBranchNotAssigned()
    {
        var user = new CurrentUserServiceStub
        {
            Role = "BranchAccountant",
            BranchIds = [1, 5]
        };

        var error = ReportBranchAccess.DenyIfOutOfScope(user, branchId: 7);

        Assert.True(error.HasValue);
        Assert.Equal(ReportErrors.BranchAccessDenied.Code, error.Value.Code);
    }
}
