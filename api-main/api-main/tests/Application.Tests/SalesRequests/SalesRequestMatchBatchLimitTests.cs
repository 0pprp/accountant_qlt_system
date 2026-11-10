using Application.SalesRequests.Common;

namespace Application.Tests.SalesRequests;

public class SalesRequestMatchBatchLimitTests
{
    [Fact]
    public void Declared_batch_limit_is_fifty()
    {
        Assert.Equal(50, SalesRequestMatchBatchLimits.MaxRequestIds);
    }

    [Fact]
    public void Over_limit_error_code_is_stable()
    {
        Assert.Equal("SalesRequest_Batch_Limit_Exceeded", SalesRequestErrors.BatchLimitExceeded.Code);
    }
}
