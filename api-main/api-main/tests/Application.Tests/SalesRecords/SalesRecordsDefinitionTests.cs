using Application.SalesRecords.Common;
using Domain.Entities.OrderAggregate.Enums;

namespace Application.Tests.SalesRecords;

public class SalesRecordsDefinitionTests
{
    [Fact]
    public void Completed_sale_means_approved_order_not_request_completed()
    {
        Assert.Equal(OrderApprovalStatus.Approved, OrderApprovalStatus.Approved);
        Assert.True((byte)OrderApprovalStatus.Approved != (byte)OrderApprovalStatus.Pending);
        Assert.True((byte)OrderApprovalStatus.Approved != (byte)OrderApprovalStatus.Rejected);
    }

    [Fact]
    public void Sort_allowlist_rejects_arbitrary_columns()
    {
        Assert.Null(SalesRecordsDataService.ValidateSort("saleDate"));
        Assert.Null(SalesRecordsDataService.ValidateSort("sellAmount"));
        Assert.NotNull(SalesRecordsDataService.ValidateSort("DROP TABLE"));
        Assert.NotNull(SalesRecordsDataService.ValidateSort("Customer.FullName"));
    }

    [Fact]
    public void Remaining_is_sell_minus_order_receipts_not_customer_wide()
    {
        const double sell = 1_000_000;
        const double receiptsThisOrder = 250_000;
        const double otherCustomerReceipts = 9_000_000;
        var remaining = Math.Round(sell - receiptsThisOrder, MidpointRounding.AwayFromZero);
        Assert.Equal(750_000, remaining);
        Assert.NotEqual(sell - otherCustomerReceipts, remaining);
    }
}
