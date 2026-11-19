using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesMatching;

namespace Application.DelegateFollowup.Common;

public static class DelegateSalesOrdersRules
{
    public const string OutcomeSold = "sold";
    public const string OutcomeRejected = "rejected";
    public const string OutcomeOpen = "open";
    public const int WrongSaleMaxShopScore = 6;
    public const string DeclaredOldReason = "مبيع قديم (زبون موجود)";
    public const string SamePhoneReason = "نفس رقم الهاتف";
    public const string SameTripleNameReason = "نفس الاسم الثلاثي";
    public const string NoEvaluationLabel = "بدون تقييم";

    public static string Outcome(SalesRequestStatus status, bool isDeleted)
    {
        if (status == SalesRequestStatus.Completed)
            return OutcomeSold;
        if (status == SalesRequestStatus.Rejected || isDeleted)
            return OutcomeRejected;
        return OutcomeOpen;
    }

    public static string StatusLabel(string outcome, SalesRequestStatus status) => outcome switch
    {
        OutcomeSold => "صار مبيع",
        OutcomeRejected => "مرفوض",
        _ => status switch
        {
            SalesRequestStatus.New => "جديد",
            SalesRequestStatus.Assigned => "محوّل لموظف",
            SalesRequestStatus.Viewed => "تمت المشاهدة",
            SalesRequestStatus.Pending => "معلق",
            SalesRequestStatus.PreparedForSale => "جاهز للبيع",
            SalesRequestStatus.InProgress => "قيد البيع",
            SalesRequestStatus.ConvertedToSale => "قيد البيع",
            SalesRequestStatus.Inspected => "تم الكشف",
            SalesRequestStatus.Returned => "معاد",
            SalesRequestStatus.LegalFollowup => "متابعة قانونية",
            _ => "قيد العمل"
        }
    };

    public static int? ResolveListId(int? linkedOrderListId, IReadOnlyList<int> creatorListsByPreference)
    {
        if (linkedOrderListId is > 0)
            return linkedOrderListId;
        return creatorListsByPreference.Count == 0 ? null : creatorListsByPreference[0];
    }

    public static IReadOnlyList<int> OrderCreatorLists(
        IEnumerable<(int ListId, DateOnly? LastSaleOnOrBeforeRequest)> candidates)
        => candidates
            .OrderByDescending(c => c.LastSaleOnOrBeforeRequest.HasValue)
            .ThenByDescending(c => c.LastSaleOnOrBeforeRequest)
            .ThenBy(c => c.ListId)
            .Select(c => c.ListId)
            .ToList();

    public static bool IsWrongSale(int? shopScore) => shopScore is >= 1 and <= WrongSaleMaxShopScore;

    public static bool InMonth(DateOnly? date, DateOnly today)
        => date is { } d && d.Year == today.Year && d.Month == today.Month;

    public static string? TripleKey(string? name)
    {
        var tokens = SalesRequestNameSimilarity.MatchingTokens(name);
        return tokens.Count < 3 ? null : $"{tokens[0]}|{tokens[1]}|{tokens[2]}";
    }

    public sealed record ExistingCustomer(int CustomerId, string? Name, string? Phone);

    public sealed class CustomerIndex
    {
        private readonly Dictionary<string, List<ExistingCustomer>> _byTriple = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ExistingCustomer>> _byPhone = new(StringComparer.Ordinal);

        public CustomerIndex(IEnumerable<ExistingCustomer> customers)
        {
            foreach (var customer in customers)
            {
                var triple = TripleKey(customer.Name);
                if (triple is not null)
                    Add(_byTriple, triple, customer);

                var phone = SalesPhoneNormalizer.ForMatch(customer.Phone);
                if (string.IsNullOrWhiteSpace(phone) == false)
                    Add(_byPhone, phone, customer);
            }
        }

        public (ExistingCustomer Customer, string Reason)? FindEarlier(string? name, string? phone, int? ownCustomerId)
        {
            bool Earlier(ExistingCustomer c) =>
                c.CustomerId != ownCustomerId && (ownCustomerId is not > 0 || c.CustomerId < ownCustomerId);

            var normalizedPhone = SalesPhoneNormalizer.ForMatch(phone);
            if (string.IsNullOrWhiteSpace(normalizedPhone) == false
                && _byPhone.TryGetValue(normalizedPhone, out var byPhone))
            {
                var hit = byPhone.Where(Earlier).OrderBy(c => c.CustomerId).FirstOrDefault();
                if (hit is not null)
                    return (hit, SamePhoneReason);
            }

            var triple = TripleKey(name);
            if (triple is not null && _byTriple.TryGetValue(triple, out var byName))
            {
                var hit = byName.Where(Earlier).OrderBy(c => c.CustomerId).FirstOrDefault();
                if (hit is not null)
                    return (hit, SameTripleNameReason);
            }

            return null;
        }

        private static void Add(Dictionary<string, List<ExistingCustomer>> map, string key, ExistingCustomer customer)
        {
            if (map.TryGetValue(key, out var list) == false)
            {
                list = [];
                map[key] = list;
            }

            list.Add(customer);
        }
    }

    public static readonly IReadOnlyList<string> PaymentRatingLabels =
    [
        DelegateFollowupRules.LabelExcellent,
        DelegateFollowupRules.LabelGood,
        DelegateFollowupRules.LabelWeak,
        DelegateFollowupRules.LabelLagging,
        DelegateFollowupRules.LabelBad
    ];

    public static void Summarize(DelegateSalesOrdersDto dto, IReadOnlyList<DelegateSalesOrderDto> orders, DateOnly today)
    {
        var sold = orders.Where(o => o.Outcome == OutcomeSold).ToList();
        dto.Orders = orders.ToList();
        dto.Total = orders.Count;
        dto.Sold = sold.Count;
        dto.Rejected = orders.Count(o => o.Outcome == OutcomeRejected);
        dto.Open = orders.Count(o => o.Outcome == OutcomeOpen);
        var decided = dto.Sold + dto.Rejected;
        dto.ConversionPercent = decided == 0 ? 0 : Math.Round(dto.Sold * 100.0 / decided, 1);
        dto.NewCustomerSales = sold.Count(o => o.IsNewCustomer == true);
        dto.NewCustomerSalesThisMonth = sold.Count(o => o.IsNewCustomer == true && InMonth(o.SoldAtIraq, today));
        dto.OldCustomerSales = sold.Count(o => o.IsNewCustomer == false);
        dto.SoldThisMonth = sold.Count(o => InMonth(o.SoldAtIraq, today));
        dto.OrdersThisMonth = orders.Count(o => InMonth(o.CreatedAtIraq, today));
        dto.WrongSales = orders.Count(o => o.IsWrongSale);
        dto.TotalSoldValue = Math.Round(sold.Sum(o => o.FinalSalePrice ?? 0), 0, MidpointRounding.AwayFromZero);
        var shop = orders.Where(o => o.ShopRating is >= 1 and <= 10).Select(o => (double)o.ShopRating!.Value).ToList();
        dto.AverageShopRating = shop.Count == 0 ? null : Math.Round(shop.Average(), 1);
        var days = sold
            .Where(o => o.SoldAtIraq is not null && o.CreatedAtIraq is not null)
            .Select(o => (double)Math.Max(0, o.SoldAtIraq!.Value.DayNumber - o.CreatedAtIraq!.Value.DayNumber))
            .ToList();
        dto.AverageDaysToSale = days.Count == 0 ? null : Math.Round(days.Average(), 1);
        dto.Evaluations =
        [
            new DelegateSalesOrdersCountDto { Key = "0", Label = NoEvaluationLabel, Count = orders.Count }
        ];
        dto.PaymentRatings = PaymentRatingLabels
            .Select(l => new DelegateSalesOrdersCountDto
            {
                Key = l,
                Label = l,
                Count = sold.Count(o => o.PaymentRatingLabel == l)
            })
            .ToList();
    }
}

public class DelegateSalesOrderDto
{
    public int RequestId { get; set; }
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public DateOnly? CreatedAtIraq { get; set; }
    public string Outcome { get; set; } = DelegateSalesOrdersRules.OutcomeOpen;
    public string StatusLabel { get; set; } = "";
    public string? RejectionReason { get; set; }
    public string? TargetEmployeeName { get; set; }
    public int? ShopRating { get; set; }
    public bool IsWrongSale { get; set; }
    public int EvaluationLevel { get; set; }
    public string EvaluationLabel { get; set; } = DelegateSalesOrdersRules.NoEvaluationLabel;
    public int? CustomerId { get; set; }
    public DateOnly? SoldAtIraq { get; set; }
    public double? FinalSalePrice { get; set; }
    public bool? IsNewCustomer { get; set; }
    public string? OldCustomerReason { get; set; }
    public int? MatchedCustomerId { get; set; }
    public string? MatchedCustomerName { get; set; }
    public string? PaymentRatingLabel { get; set; }
    public bool IsDeclaredOld { get; set; }
}

public class DelegateSalesOrdersCountDto
{
    public required string Key { get; set; }
    public required string Label { get; set; }
    public int Count { get; set; }
}

public class DelegateSalesOrdersDto
{
    public int ListId { get; set; }
    public int Total { get; set; }
    public int Sold { get; set; }
    public int Rejected { get; set; }
    public int Open { get; set; }
    public double ConversionPercent { get; set; }
    public int NewCustomerSales { get; set; }
    public int NewCustomerSalesThisMonth { get; set; }
    public int OldCustomerSales { get; set; }
    public int SoldThisMonth { get; set; }
    public int OrdersThisMonth { get; set; }
    public int WrongSales { get; set; }
    public double TotalSoldValue { get; set; }
    public double? AverageShopRating { get; set; }
    public double? AverageDaysToSale { get; set; }
    public List<DelegateSalesOrdersCountDto> Evaluations { get; set; } = [];
    public List<DelegateSalesOrdersCountDto> PaymentRatings { get; set; } = [];
    public List<DelegateSalesOrderDto> Orders { get; set; } = [];
}
