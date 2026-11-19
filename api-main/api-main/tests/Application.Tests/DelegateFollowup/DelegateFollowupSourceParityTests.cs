using Application.DelegateFollowup.Common;
using Domain.Entities.DelegateFollowupAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.Tests.DelegateFollowup;

public class DelegateFollowupSourceParityTests
{
    [Theory]
    [InlineData("2026-11-21", "2026-11-19")]
    [InlineData("2026-11-22", "2026-11-21")]
    [InlineData("2026-11-20", "2026-11-19")]
    [InlineData("2026-11-19", "2026-11-18")]
    public void ReportedYesterday_SkipsFriday(string today, string expected)
        => Assert.Equal(DateOnly.Parse(expected), DelegateFollowupRules.ReportedYesterday(DateOnly.Parse(today)));

    [Fact]
    public void HistoryDays_EndAtReportedYesterday()
    {
        var saturday = new DateOnly(2026, 11, 21);
        var days = DelegateFollowupRules.LastNCalendarDaysEndingYesterday(saturday, 30);
        Assert.Equal(30, days.Count);
        Assert.Equal(new DateOnly(2026, 11, 19), days[0]);
        Assert.Equal(new DateOnly(2026, 10, 21), days[^1]);
    }

    [Theory]
    [InlineData(499, false)]
    [InlineData(500, true)]
    [InlineData(1_200, true)]
    [InlineData(0, false)]
    [InlineData(-800, false)]
    public void OpenBalance_UsesThousandRounding(double remaining, bool expected)
        => Assert.Equal(expected, DelegateFollowupRules.IsOpenBalance(remaining));

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public void KeepSettledCustomer_StaysTwoDays(int daysAgo, bool expected)
    {
        var today = new DateOnly(2026, 11, 18);
        Assert.Equal(expected, DelegateFollowupRules.KeepSettledCustomer(0, 500_000, today.AddDays(-daysAgo), today));
    }

    [Fact]
    public void KeepSettledCustomer_IgnoresOpenBalanceAndNeverSold()
    {
        var today = new DateOnly(2026, 11, 18);
        Assert.False(DelegateFollowupRules.KeepSettledCustomer(50_000, 500_000, today, today));
        Assert.False(DelegateFollowupRules.KeepSettledCustomer(0, 0, today, today));
        Assert.False(DelegateFollowupRules.KeepSettledCustomer(0, 500_000, null, today));
    }

    [Theory]
    [InlineData("2026-09-22", 520_000, "2026-11-17", 58)]
    [InlineData("2026-07-23", 76_000, "2026-11-03", 119)]
    [InlineData("2026-11-08", 2_735_000, "2026-11-14", 11)]
    [InlineData("2026-08-01", 0, "2026-09-10", 41)]
    public void DeviceDays_MatchesOldViewColumn(string sale, double remaining, string lastPayment, int expected)
        => Assert.Equal(expected, DelegateFollowupRules.DeviceDays(
            DateOnly.Parse(sale), remaining, DateOnly.Parse(lastPayment), new DateOnly(2026, 11, 18)));

    [Fact]
    public void ActiveCustomer_IsThirtyDaysInclusive()
    {
        var today = new DateOnly(2026, 11, 18);
        Assert.True(DelegateFollowupRules.IsActiveCustomer(today.AddDays(-30), today));
        Assert.False(DelegateFollowupRules.IsActiveCustomer(today.AddDays(-31), today));
        Assert.False(DelegateFollowupRules.IsActiveCustomer(null, today));
    }

    [Fact]
    public void CountDistinctPayers_CountsCustomersNotPayments()
    {
        var day = new DateOnly(2026, 11, 17);
        var payments = new List<(int, DateOnly, double)>
        {
            (1, day, 10_000), (1, day, 5_000), (2, day, 7_000), (3, day.AddDays(-1), 9_000), (4, day, 0)
        };
        Assert.Equal(2, DelegateFollowupRules.CountDistinctPayers(payments, new HashSet<int> { 1, 2, 3, 4 }, day));
    }

    [Theory]
    [InlineData(6, true)]
    [InlineData(0, true)]
    [InlineData(7, false)]
    public void BoardNote_RequiredAtSixAndBelow(int score, bool required)
        => Assert.Equal(required, DelegateFollowupRules.BoardOperatorNoteIsRequired(score));

    [Theory]
    [InlineData(null, "head")]
    [InlineData("", "head")]
    [InlineData("assistant", "head")]
    [InlineData("Accountant", "accountant")]
    [InlineData(" accountant ", "accountant")]
    public void RatingSlot_Normalizes(string? input, string expected)
        => Assert.Equal(expected, DirectorBoardOperatorListRating.NormalizeSlot(input));

    [Fact]
    public void Outcome_Buckets()
    {
        Assert.Equal(DelegateSalesOrdersRules.OutcomeSold, DelegateSalesOrdersRules.Outcome(SalesRequestStatus.Completed, false));
        Assert.Equal(DelegateSalesOrdersRules.OutcomeRejected, DelegateSalesOrdersRules.Outcome(SalesRequestStatus.Rejected, false));
        Assert.Equal(DelegateSalesOrdersRules.OutcomeRejected, DelegateSalesOrdersRules.Outcome(SalesRequestStatus.Assigned, true));
        Assert.Equal(DelegateSalesOrdersRules.OutcomeOpen, DelegateSalesOrdersRules.Outcome(SalesRequestStatus.ConvertedToSale, false));
    }

    [Fact]
    public void ResolveListId_LinkedOrderWins()
        => Assert.Equal(7, DelegateSalesOrdersRules.ResolveListId(7, [3, 4]));

    [Fact]
    public void ResolveListId_DelegateWithSeveralLists_PicksExactlyOne()
    {
        var ordered = DelegateSalesOrdersRules.OrderCreatorLists(
        [
            (3, new DateOnly(2026, 10, 1)),
            (4, new DateOnly(2026, 11, 2)),
            (5, null)
        ]);
        Assert.Equal([4, 3, 5], ordered);
        Assert.Equal(4, DelegateSalesOrdersRules.ResolveListId(null, ordered));
    }

    [Fact]
    public void ResolveListId_NoActivity_FallsBackToLowestListId()
    {
        var ordered = DelegateSalesOrdersRules.OrderCreatorLists([(9, null), (2, null)]);
        Assert.Equal(2, DelegateSalesOrdersRules.ResolveListId(null, ordered));
        Assert.Null(DelegateSalesOrdersRules.ResolveListId(null, []));
    }

    [Fact]
    public void CustomerIndex_PhoneMatchBeatsName_AndIgnoresOwnOrLaterRows()
    {
        var index = new DelegateSalesOrdersRules.CustomerIndex(
        [
            new(10, "احمد محمد حمود", "07701234567"),
            new(20, "سجاد احسان احمد", "07800000000"),
            new(30, "زيد علي حسن", "07711111111")
        ]);

        var phone = index.FindEarlier("اسم مختلف تماما", "07701234567", 25);
        Assert.Equal((10, DelegateSalesOrdersRules.SamePhoneReason), (phone!.Value.Customer.CustomerId, phone.Value.Reason));

        var name = index.FindEarlier("سجاد احسان احمد الرابع", null, 25);
        Assert.Equal((20, DelegateSalesOrdersRules.SameTripleNameReason), (name!.Value.Customer.CustomerId, name.Value.Reason));

        Assert.Null(index.FindEarlier("زيد علي حسن", "07711111111", 30));
        Assert.Null(index.FindEarlier("زيد علي حسن", null, 25));
        Assert.Null(index.FindEarlier("احمد محمد", null, 99));
    }

    [Fact]
    public void Summarize_CountsMatchCardFilters()
    {
        var today = new DateOnly(2026, 11, 18);
        var orders = new List<DelegateSalesOrderDto>
        {
            new() { RequestId = 1, Outcome = "sold", IsNewCustomer = true, SoldAtIraq = today, CreatedAtIraq = today.AddDays(-2), FinalSalePrice = 1_000_000, ShopRating = 8, PaymentRatingLabel = "ممتاز" },
            new() { RequestId = 2, Outcome = "sold", IsNewCustomer = false, SoldAtIraq = new DateOnly(2026, 10, 5), CreatedAtIraq = new DateOnly(2026, 10, 5), FinalSalePrice = 500_000, ShopRating = 4, IsWrongSale = true },
            new() { RequestId = 3, Outcome = "rejected", CreatedAtIraq = today },
            new() { RequestId = 4, Outcome = "open", CreatedAtIraq = new DateOnly(2026, 9, 1) }
        };

        var dto = new DelegateSalesOrdersDto();
        DelegateSalesOrdersRules.Summarize(dto, orders, today);

        Assert.Equal(4, dto.Total);
        Assert.Equal(2, dto.Sold);
        Assert.Equal(1, dto.Rejected);
        Assert.Equal(1, dto.Open);
        Assert.Equal(66.7, dto.ConversionPercent);
        Assert.Equal(1, dto.NewCustomerSales);
        Assert.Equal(1, dto.NewCustomerSalesThisMonth);
        Assert.Equal(1, dto.OldCustomerSales);
        Assert.Equal(1, dto.SoldThisMonth);
        Assert.Equal(2, dto.OrdersThisMonth);
        Assert.Equal(1, dto.WrongSales);
        Assert.Equal(1_500_000, dto.TotalSoldValue);
        Assert.Equal(6.0, dto.AverageShopRating);
        Assert.Equal(1.0, dto.AverageDaysToSale);
        Assert.Equal(1, dto.PaymentRatings.Single(p => p.Key == "ممتاز").Count);
    }

    [Fact]
    public void Summarize_NoDecisions_ZeroConversion()
    {
        var dto = new DelegateSalesOrdersDto();
        DelegateSalesOrdersRules.Summarize(dto, [new DelegateSalesOrderDto { Outcome = "open" }], new DateOnly(2026, 11, 18));
        Assert.Equal(0, dto.ConversionPercent);
        Assert.Null(dto.AverageShopRating);
        Assert.Null(dto.AverageDaysToSale);
    }
}
