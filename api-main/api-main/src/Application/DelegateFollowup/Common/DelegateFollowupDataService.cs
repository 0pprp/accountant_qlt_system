using Application.Common.Interfaces;
using Application.SalesRatings.Common;
using Domain.Entities.DelegateFollowupAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.DelegateFollowup.Common;

public class DelegateFollowupDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public DelegateFollowupDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<DelegateFollowupListCardDto>> ListActiveListsAsync(
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return [];

        var asOf = _clock.Today;
        var cutoff = asOf.AddDays(-DelegateFollowupRules.MaxDaysListActivity);

        var lastPayments = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Date >= cutoff
                        && p.Order!.OrderListId != null
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && branchIds.Contains(p.Order.BranchId)
                        && branchIds.Contains(p.Order.OrderList!.BranchId))
            .GroupBy(p => p.Order!.OrderListId!.Value)
            .Select(g => new { ListId = g.Key, LastPaymentDate = g.Max(p => p.Date) })
            .ToListAsync(ct);

        var activeListIds = lastPayments
            .Where(x => DelegateFollowupRules.IsListActive(x.LastPaymentDate, asOf))
            .Select(x => x.ListId)
            .ToList();
        if (activeListIds.Count == 0)
            return [];

        var lastPaymentByList = lastPayments.ToDictionary(x => x.ListId, x => x.LastPaymentDate);

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(l => activeListIds.Contains(l.Id) && branchIds.Contains(l.BranchId))
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.BranchId,
                DelegateName = l.Mandob != null ? l.Mandob.FullName : null,
                FollowerName = l.Motaba != null ? l.Motaba.FullName : null
            })
            .ToListAsync(ct);
        if (lists.Count == 0)
            return [];

        var scopedListIds = lists.Select(l => l.Id).ToList();
        var orderRows = await LoadOrderRowsAsync(
            _db.Orders.AsNoTracking()
                .Where(o => o.OrderListId != null
                            && scopedListIds.Contains(o.OrderListId.Value)
                            && branchIds.Contains(o.BranchId)
                            && o.ApprovalStatus == OrderApprovalStatus.Approved),
            false,
            ct);

        var factsByList = orderRows
            .GroupBy(r => r.OrderListId)
            .ToDictionary(
                g => g.Key,
                g => BuildFacts(g).Where(f => f.Remaining > 0).ToList());

        var cards = new List<DelegateFollowupListCardDto>(lists.Count);
        foreach (var list in lists)
        {
            if (factsByList.TryGetValue(list.Id, out var facts) == false || facts.Count == 0)
                continue;

            var snapshot = TakeSnapshot(facts, asOf);
            cards.Add(new DelegateFollowupListCardDto
            {
                ListId = list.Id,
                ListName = string.IsNullOrWhiteSpace(list.Name) ? $"قائمة {list.Id}" : list.Name.Trim(),
                BranchId = list.BranchId,
                DelegateName = list.DelegateName,
                FollowerName = list.FollowerName,
                RealRating = snapshot.AverageScore,
                RealRatingLabel = snapshot.Label,
                NonZeroedCustomerCount = facts.Count,
                LastPaymentDate = lastPaymentByList.GetValueOrDefault(list.Id)
            });
        }

        return cards
            .OrderByDescending(c => c.LastPaymentDate)
            .ThenBy(c => c.ListName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<DelegateFollowupSummaryDto?> GetSummaryAsync(
        int listId,
        int branchId,
        CancellationToken ct)
    {
        var list = await LoadListMetaAsync(listId, branchId, ct);
        if (list is null)
            return null;

        var asOf = _clock.Today;
        var weekAgo = DelegateFollowupRules.WeekAgoDate(asOf);
        var yesterday = asOf.AddDays(-1);
        var lastFriday = DelegateFollowupRules.LastFridayBefore(asOf);
        var historyDays = DelegateFollowupRules.LastNCalendarDaysEndingYesterday(
            asOf,
            DelegateFollowupRules.CollectedHistoryDays);
        var fridayHistory = DelegateFollowupRules.LastNFridays(asOf, DelegateFollowupRules.CollectedHistoryDays);

        var orderRows = await LoadOrderRowsAsync(ListOrders(listId, branchId), false, ct);
        var allFacts = BuildFacts(orderRows);
        var facts = allFacts.Where(f => f.Remaining > 0).ToList();

        var customerNames = await LoadCustomerNamesAsync(facts.Select(f => f.CustomerId).ToList(), ct);
        ApplyLegalFlags(facts, customerNames);

        var nonLegalFacts = facts.Where(f => f.IsLegal == false).ToList();
        var snapshot = TakeSnapshot(facts, asOf);
        var weekSnapshot = TakeSnapshot(facts, weekAgo);
        var tierSnapshot = TakeSnapshot(nonLegalFacts, asOf);
        var remainingBands = DelegateFollowupRules.CountRemainingBands(facts.Select(f => f.Remaining));

        var requiredDue = DelegateFollowupRules.SumRequiredDueInstallment(
            facts.Select(f => (f.AmountDaySales, f.Remaining, f.IsLegal)));

        var historyFrom = historyDays[^1];
        var fridayFrom = fridayHistory[^1];
        var rangeFrom = fridayFrom < historyFrom ? fridayFrom : historyFrom;
        var paymentRows = await LoadCollectedPaymentRowsAsync(listId, branchId, rangeFrom, yesterday, ct);

        var paymentsByDay = paymentRows
            .GroupBy(p => p.Date)
            .ToDictionary(g => g.Key, g => Math.Round(g.Sum(x => x.Amount), 0, MidpointRounding.AwayFromZero));

        double DayAmount(DateOnly d) => paymentsByDay.GetValueOrDefault(d);

        var labelCache = new Dictionary<DateOnly, Dictionary<int, string>>();

        Dictionary<int, string> LabelsOn(DateOnly day)
        {
            if (labelCache.TryGetValue(day, out var cached))
                return cached;

            var map = new Dictionary<int, string>(facts.Count);
            foreach (var f in facts)
            {
                if (f.IsLegal)
                {
                    map[f.CustomerId] = DelegateFollowupRules.StatusLegal;
                    continue;
                }

                var score = ScoreAt(f, day);
                map[f.CustomerId] = score is null
                    ? DelegateFollowupRules.StatusUnknown
                    : DelegateFollowupRules.NormalizeStatusLabel(CustomerRatingCalculator.LevelName(score.Value));
            }

            labelCache[day] = map;
            return map;
        }

        string StatusFor(CustomerFact f, DateOnly day) => LabelsOn(day)[f.CustomerId];

        IEnumerable<(string? Prev, string? Cur)> PairsForDay(DateOnly day) =>
            facts.Select(f => (
                Prev: (string?)StatusFor(f, day.AddDays(-1)),
                Cur: (string?)StatusFor(f, day)));

        List<DelegateFollowupConversionCustomerDto> ArrivalCustomers(DateOnly day, string target)
        {
            var arrivals = new List<DelegateFollowupConversionCustomerDto>();
            foreach (var f in facts)
            {
                var prev = StatusFor(f, day.AddDays(-1));
                var cur = StatusFor(f, day);
                if (cur != target || prev == cur)
                    continue;

                var name = customerNames.GetValueOrDefault(f.CustomerId)?.FullName;
                arrivals.Add(new DelegateFollowupConversionCustomerDto
                {
                    CustomerId = f.CustomerId,
                    CustomerName = string.IsNullOrWhiteSpace(name) ? $"#{f.CustomerId}" : name,
                    FromLabel = prev
                });
            }

            return arrivals
                .OrderBy(x => x.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        var conversionTargets = DelegateFollowupRules.TierLabels
            .Append(DelegateFollowupRules.StatusLegal)
            .ToList();
        var todayConversion = DelegateFollowupRules.CountConversions(
            PairsForDay(asOf).Where(p =>
                p.Prev != DelegateFollowupRules.StatusLegal && p.Cur != DelegateFollowupRules.StatusLegal));
        var todayArrivals = DelegateFollowupRules.BuildArrivalBreakdown(PairsForDay(asOf), conversionTargets);

        var conversionTargetDtos = new List<DelegateFollowupConversionTargetDto>(conversionTargets.Count);
        foreach (var target in conversionTargets)
        {
            var todayFrom = todayArrivals.GetValueOrDefault(target) ?? [];
            var todayCustomers = ArrivalCustomers(asOf, target);

            var targetHistory = historyDays.Select(d =>
            {
                var dayMap = DelegateFollowupRules.BuildArrivalBreakdown(PairsForDay(d), conversionTargets);
                var from = dayMap.GetValueOrDefault(target) ?? [];
                var dayCustomers = ArrivalCustomers(d, target);
                return new DelegateFollowupConversionTargetDayDto
                {
                    Date = d,
                    ArrivedCount = dayCustomers.Count,
                    FromBreakdown = from
                        .Select(x => new DelegateFollowupConversionFromDto { FromLabel = x.From, Count = x.Count })
                        .ToList(),
                    Customers = dayCustomers
                };
            }).ToList();

            conversionTargetDtos.Add(new DelegateFollowupConversionTargetDto
            {
                TargetLabel = target,
                ArrivedCount = todayCustomers.Count,
                DetailText = FormatArrivalDetail(todayFrom),
                FromBreakdown = todayFrom
                    .Select(x => new DelegateFollowupConversionFromDto { FromLabel = x.From, Count = x.Count })
                    .ToList(),
                Customers = todayCustomers,
                Last30Days = targetHistory
            });
        }

        var conversion = new DelegateFollowupConversionDto
        {
            Improved = todayConversion.Improved,
            Declined = todayConversion.Declined,
            Unchanged = todayConversion.Unchanged,
            SummaryText =
                $"ازداد {todayConversion.Improved} · نقص {todayConversion.Declined} · مساوي {todayConversion.Unchanged}",
            LegalTargetAvailable = true,
            Targets = conversionTargetDtos
        };

        var todayLabels = LabelsOn(asOf);
        var tierMoney = new List<DelegateFollowupTierMoneyDto>(DelegateFollowupRules.TierLabels.Count);
        foreach (var label in DelegateFollowupRules.TierLabels)
        {
            var tierFacts = facts.Where(f => f.IsLegal == false && todayLabels[f.CustomerId] == label).ToList();
            var tierDue = DelegateFollowupRules.SumRequiredDueInstallment(
                tierFacts.Select(f => (f.AmountDaySales, f.Remaining, f.IsLegal)));

            var tierIds = tierFacts.Select(f => f.CustomerId).ToHashSet();
            var tierByDay = paymentRows
                .Where(p => tierIds.Contains(p.CustomerId))
                .GroupBy(p => p.Date)
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(x => x.Amount), 0, MidpointRounding.AwayFromZero));

            double TierDay(DateOnly d) => tierByDay.GetValueOrDefault(d);

            tierMoney.Add(new DelegateFollowupTierMoneyDto
            {
                Label = label,
                RequiredDueAvailable = true,
                RequiredDue = tierDue,
                CollectedYesterday = TierDay(yesterday),
                CollectedLast30Days = historyDays
                    .Select(d => new DelegateFollowupDayAmountDto { Date = d, Amount = TierDay(d) })
                    .ToList()
            });
        }

        var boardByDay = await LoadBoardRatingsRangeAsync(listId, historyFrom, asOf, ct);
        var followerCommittee = await LoadFollowerCommitteeAsync(listId, historyFrom, asOf, ct);
        var supervisionCommittee = await LoadSupervisionCommitteeAsync(listId, historyFrom, asOf, ct);
        var salesDelegate = await LoadSalesDelegateCommitteeAsync(listId, historyFrom, asOf, ct);
        var realRatingLast30 = historyDays.Select(d =>
        {
            var snap = TakeSnapshot(facts, d);
            return new DelegateFollowupDayRatingDto { Date = d, Score = snap.AverageScore, Label = snap.Label };
        }).ToList();

        var boardLast30 = historyDays.Select(d =>
        {
            boardByDay.TryGetValue(d, out var row);
            return new DelegateFollowupDayRatingDto
            {
                Date = d,
                Score = row?.Score ?? 0,
                Label = row is null ? "" : $"{row.Score}/10",
                BoardOperatorScore = row?.Score,
                BoardOperatorNote = row?.Note
            };
        }).ToList();

        boardByDay.TryGetValue(asOf, out var todayBoard);

        return new DelegateFollowupSummaryDto
        {
            ListId = listId,
            ListName = list.Name,
            BranchId = list.BranchId,
            AsOfDate = asOf,
            RealRating = snapshot.AverageScore,
            RealRatingLabel = snapshot.Label,
            RealRatingWeekAgo = weekSnapshot.AverageScore,
            RatingTrend = DelegateFollowupRules.ComputeScoreTrend(snapshot.AverageScore, weekSnapshot.AverageScore),
            NonZeroedCustomerCount = facts.Count,
            LegalAvailable = true,
            LegalCustomerCount = facts.Count(f => f.IsLegal),
            ExcellentCount = tierSnapshot.ExcellentCount,
            GoodCount = tierSnapshot.GoodCount,
            WeakCount = tierSnapshot.WeakCount,
            LaggingCount = tierSnapshot.LaggingCount,
            BadCount = tierSnapshot.BadCount,
            RemainingLt100Count = remainingBands.Lt100k,
            Remaining100To200Count = remainingBands.From100To200k,
            RemainingGe100Count = remainingBands.Ge100k,
            NewSaleUnpaid3DaysCount = facts.Count(f => IsNewSaleUnpaid(f, asOf)),
            RequiredDueAvailable = true,
            RequiredDueToday = requiredDue,
            RequiredDueUnavailableReason = null,
            CollectedYesterday = DayAmount(yesterday),
            CollectedLastFriday = DayAmount(lastFriday),
            LastFridayDate = lastFriday,
            CollectedLast30Days = historyDays
                .Select(d => new DelegateFollowupDayAmountDto { Date = d, Amount = DayAmount(d) })
                .ToList(),
            CollectedLast30Fridays = fridayHistory
                .Select(d => new DelegateFollowupDayAmountDto { Date = d, Amount = DayAmount(d) })
                .ToList(),
            TierMoney = tierMoney,
            RealRatingLast30Days = realRatingLast30,
            BoardOperatorLast30Days = boardLast30,
            FollowerCommittee = followerCommittee,
            SupervisionCommittee = supervisionCommittee,
            SalesDelegate = salesDelegate,
            Conversion = conversion,
            BoardOperatorRating = MapBoardRating(listId, asOf, todayBoard),
            UnresolvedNotes =
            [
                new()
                {
                    Field = "OrderList.ActiveState",
                    Note = "لا مكافئ لـ DelegateState على OrderList — النشاط يعتمد على تسديد ≤40 يوماً فقط"
                }
            ]
        };
    }

    public async Task<DelegateFollowupCustomersDto?> GetCustomersAsync(
        int listId,
        int branchId,
        CancellationToken ct)
    {
        var list = await LoadListMetaAsync(listId, branchId, ct);
        if (list is null)
            return null;

        var asOf = _clock.Today;
        var orderRows = await LoadOrderRowsAsync(ListOrders(listId, branchId), true, ct);
        var facts = BuildFacts(orderRows).Where(f => f.Remaining > 0).ToList();

        var names = await LoadCustomerNamesAsync(facts.Select(f => f.CustomerId).ToList(), ct);
        ApplyLegalFlags(facts, names);
        var orderRowsByCustomer = orderRows.ToLookup(r => r.CustomerId);

        var customers = new List<DelegateFollowupCustomerDto>(facts.Count);
        foreach (var f in facts)
        {
            names.TryGetValue(f.CustomerId, out var info);
            var rows = orderRowsByCustomer[f.CustomerId].ToList();
            var score = ScoreAt(f, asOf);
            var days = f.SaleDate is null
                ? 0
                : CustomerRatingCalculator.DiffDaysInclusive(f.SaleDate.Value, f.LastPaymentDate ?? asOf);
            var rate = f.Total <= 0 || days <= 0 ? 0 : f.Receipts * 1000.0 / (f.Total * days);
            var itemNames = rows
                .SelectMany(r => r.ItemNames)
                .Where(n => string.IsNullOrWhiteSpace(n) == false)
                .Select(n => n!.Trim())
                .Distinct();

            var label = f.IsLegal
                ? DelegateFollowupRules.StatusLegal
                : score is null
                    ? ""
                    : CustomerRatingCalculator.LevelName(score.Value);

            customers.Add(new DelegateFollowupCustomerDto
            {
                CustomerId = f.CustomerId,
                ListId = listId,
                CustomerName = info?.FullName ?? $"#{f.CustomerId}",
                Address = info?.Address,
                PhoneNumber = info?.PhoneNumber,
                ShopName = info?.ShopName,
                SaleName = rows.Select(r => r.SellerName).FirstOrDefault(n => string.IsNullOrWhiteSpace(n) == false),
                DelegateName = list.DelegateName,
                FollowerName = list.FollowerName,
                DateSaleDevice = f.SaleDate,
                AmountTotalSales = f.Total,
                AmountDaySales = f.AmountDaySales,
                ReceiptsTotal = f.Receipts,
                AmountRemaining = f.Remaining,
                ItemsNames = string.Join("، ", itemNames),
                LastPaymentDate = f.LastPaymentDate,
                LastPaymentAmount = f.LastPaymentAmount,
                CountReceiptDevice = f.PaymentCount,
                NumberOfDayDevice = days,
                ReceiptRateDevice = Math.Round(rate, 2),
                Score = f.IsLegal ? null : score,
                RealRatingLabel = label,
                IsLegal = f.IsLegal,
                IsNewSaleUnpaid3Days = IsNewSaleUnpaid(f, asOf),
                Notes = null,
                ReceiptName = list.FollowerName
            });
        }

        customers = customers
            .OrderByDescending(c => c.LastPaymentDate.HasValue)
            .ThenByDescending(c => c.LastPaymentDate)
            .ThenBy(c => c.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new DelegateFollowupCustomersDto
        {
            ListId = listId,
            BranchId = list.BranchId,
            LegalAvailable = true,
            NewSaleUnpaid3DaysCount = customers.Count(c => c.IsNewSaleUnpaid3Days),
            Customers = customers,
            UnresolvedNotes = []
        };
    }

    public async Task<DelegateFollowupCustomerPaymentsDto?> GetCustomerPaymentsAsync(
        int listId,
        int branchId,
        int customerId,
        CancellationToken ct)
    {
        var list = await LoadListMetaAsync(listId, branchId, ct);
        if (list is null)
            return null;

        var onList = await _db.Orders.AsNoTracking()
            .AnyAsync(
                o => o.OrderListId == listId
                     && o.BranchId == branchId
                     && o.CustomerId == customerId
                     && o.ApprovalStatus == OrderApprovalStatus.Approved,
                ct);
        if (onList == false)
            return null;

        var payments = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Order!.OrderListId == listId
                        && p.Order.BranchId == branchId
                        && p.Order.CustomerId == customerId
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Select(p => new DelegateFollowupPaymentDto
            {
                PaymentId = p.Id,
                OrderId = p.OrderId,
                CustomerId = customerId,
                Date = p.Date,
                Amount = p.Amount,
                Description = p.Description
            })
            .ToListAsync(ct);

        return new DelegateFollowupCustomerPaymentsDto
        {
            CustomerId = customerId,
            ListId = listId,
            BranchId = branchId,
            Payments = payments,
            TotalAmount = Math.Round(payments.Sum(p => p.Amount), 0, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<bool> ListExistsInBranchAsync(int listId, int branchId, CancellationToken ct)
        => await _db.OrderLists.AsNoTracking()
            .AnyAsync(l => l.Id == listId && l.BranchId == branchId, ct);

    public async Task<DelegateFollowupBoardRatingDto> GetBoardRatingAsync(
        int listId,
        DateOnly ratingDate,
        CancellationToken ct)
    {
        var row = await _db.DirectorBoardOperatorListRatings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderListId == listId && x.RatingDate == ratingDate, ct);
        return MapBoardRating(listId, ratingDate, row);
    }

    public static DelegateFollowupBoardRatingDto MapBoardRating(
        int listId,
        DateOnly ratingDate,
        DirectorBoardOperatorListRating? row)
        => new()
        {
            ListId = listId,
            RatingDate = ratingDate,
            Score = row?.Score,
            Note = row?.Note,
            RatedByUserName = row?.RatedByUserName,
            RatedAtUtc = row?.RatedAtUtc,
            HasRating = row is not null
        };

    private IQueryable<Domain.Entities.OrderAggregate.Order> ListOrders(int listId, int branchId)
        => _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId == listId
                        && o.BranchId == branchId
                        && o.ApprovalStatus == OrderApprovalStatus.Approved);

    private async Task<ListMeta?> LoadListMetaAsync(int listId, int branchId, CancellationToken ct)
    {
        var list = await _db.OrderLists.AsNoTracking()
            .Where(l => l.Id == listId && l.BranchId == branchId)
            .Select(l => new ListMeta
            {
                Id = l.Id,
                Name = l.Name,
                BranchId = l.BranchId,
                DelegateName = l.Mandob != null ? l.Mandob.FullName : null,
                FollowerName = l.Motaba != null ? l.Motaba.FullName : null
            })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return null;

        list.Name = string.IsNullOrWhiteSpace(list.Name) ? $"قائمة {listId}" : list.Name.Trim();
        return list;
    }

    private static async Task<List<OrderRow>> LoadOrderRowsAsync(
        IQueryable<Domain.Entities.OrderAggregate.Order> source,
        bool includeItems,
        CancellationToken ct)
    {
        if (includeItems)
        {
            return await source
                .Select(o => new OrderRow
                {
                    OrderListId = o.OrderListId ?? 0,
                    CustomerId = o.CustomerId,
                    SellAmount = o.SellAmount,
                    DailyInstallmentAmount = o.DailyInstallmentAmount,
                    SaleDate = o.SaleDate,
                    Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                    PaymentCount = o.InstallmentPayments.Count(),
                    LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
                    LastPaymentAmount = o.InstallmentPayments
                        .OrderByDescending(ip => ip.Date)
                        .ThenByDescending(ip => ip.Id)
                        .Select(ip => (double?)ip.Amount)
                        .FirstOrDefault(),
                    SellerName = o.Seller != null ? o.Seller.FullName : null,
                    ItemNames = o.OrderItems.Select(i => i.ProductName).ToList()
                })
                .ToListAsync(ct);
        }

        return await source
            .Select(o => new OrderRow
            {
                OrderListId = o.OrderListId ?? 0,
                CustomerId = o.CustomerId,
                SellAmount = o.SellAmount,
                DailyInstallmentAmount = o.DailyInstallmentAmount,
                SaleDate = o.SaleDate,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                PaymentCount = o.InstallmentPayments.Count(),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
                LastPaymentAmount = o.InstallmentPayments
                    .OrderByDescending(ip => ip.Date)
                    .ThenByDescending(ip => ip.Id)
                    .Select(ip => (double?)ip.Amount)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);
    }

    private async Task<List<PaymentDayRow>> LoadCollectedPaymentRowsAsync(
        int listId,
        int branchId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        if (to < from)
            return [];

        return await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Order!.OrderListId == listId
                        && p.Order.BranchId == branchId
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && p.Date >= from
                        && p.Date <= to)
            .GroupBy(p => new { p.Order!.CustomerId, p.Date })
            .Select(g => new PaymentDayRow
            {
                CustomerId = g.Key.CustomerId,
                Date = g.Key.Date,
                Amount = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);
    }

    private async Task<Dictionary<int, CustomerInfo>> LoadCustomerNamesAsync(
        IReadOnlyList<int> customerIds,
        CancellationToken ct)
    {
        if (customerIds.Count == 0)
            return new Dictionary<int, CustomerInfo>();

        var rows = await _db.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new CustomerInfo
            {
                Id = c.Id,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Address = c.Business.Address,
                ShopName = c.Business.Name,
                IsLegal = c.IsLegal
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id);
    }

    private static void ApplyLegalFlags(
        IReadOnlyList<CustomerFact> facts,
        IReadOnlyDictionary<int, CustomerInfo> names)
    {
        foreach (var fact in facts)
        {
            if (names.TryGetValue(fact.CustomerId, out var info))
                fact.IsLegal = info.IsLegal;
        }
    }

    private async Task<Dictionary<DateOnly, DirectorBoardOperatorListRating>> LoadBoardRatingsRangeAsync(
        int listId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        var rows = await _db.DirectorBoardOperatorListRatings.AsNoTracking()
            .Where(x => x.OrderListId == listId && x.RatingDate >= from && x.RatingDate <= to)
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.RatingDate);
    }

    private static List<CustomerFact> BuildFacts(IEnumerable<OrderRow> rows)
        => rows
            .GroupBy(r => r.CustomerId)
            .Select(g =>
            {
                var lastPaymentRow = g
                    .Where(x => x.LastPaymentDate != null)
                    .OrderByDescending(x => x.LastPaymentDate)
                    .FirstOrDefault();

                return new CustomerFact
                {
                    CustomerId = g.Key,
                    Total = g.Sum(x => x.SellAmount),
                    Receipts = g.Sum(x => x.Receipts),
                    AmountDaySales = g.Sum(x => x.DailyInstallmentAmount),
                    PaymentCount = g.Sum(x => x.PaymentCount),
                    SaleDate = g.Min(x => x.SaleDate),
                    LastPaymentDate = lastPaymentRow?.LastPaymentDate,
                    LastPaymentAmount = lastPaymentRow?.LastPaymentAmount
                };
            })
            .ToList();

    private static int? ScoreAt(CustomerFact fact, DateOnly day)
        => CustomerRatingCalculator.ScoreFor(
            fact.Total,
            fact.Receipts,
            fact.Remaining,
            fact.SaleDate,
            fact.LastPaymentDate,
            day,
            false);

    private static bool IsNewSaleUnpaid(CustomerFact fact, DateOnly asOf)
        => DelegateFollowupRules.IsNewSaleUnpaid3Days(
            fact.Remaining,
            fact.Total,
            fact.Receipts,
            fact.PaymentCount,
            fact.SaleDate,
            asOf,
            fact.IsLegal);

    private async Task<DelegateFollowupCommitteeRatingDto> LoadFollowerCommitteeAsync(
        int listId,
        DateOnly from,
        DateOnly asOf,
        CancellationToken ct)
    {
        var rows = await _db.FollowerDelegateDayRatings.AsNoTracking()
            .Where(x => x.OrderListId == listId && x.RatingDate >= from && x.RatingDate <= asOf)
            .ToListAsync(ct);

        var byDay = rows
            .GroupBy(x => x.RatingDate)
            .Select(g => new
            {
                Date = g.Key,
                Score = (int)Math.Round(g.Average(x => x.Score), MidpointRounding.AwayFromZero),
                Note = g.OrderByDescending(x => x.RatedAtUtc).Select(x => x.Note).FirstOrDefault(),
                RatedBy = g.OrderByDescending(x => x.RatedAtUtc).Select(x => (string?)null).FirstOrDefault(),
                At = g.Max(x => x.RatedAtUtc)
            })
            .ToList();

        return MapCommitteeFromDays(listId, asOf, byDay.Select(x =>
            (x.Date, x.Score, x.Note, (string?)null, x.At)));
    }

    private async Task<DelegateFollowupCommitteeRatingDto> LoadSupervisionCommitteeAsync(
        int listId,
        DateOnly from,
        DateOnly asOf,
        CancellationToken ct)
    {
        var rows = await _db.ListSupervisionDayRatings.AsNoTracking()
            .Where(x => x.OrderListId == listId && x.RatingDate >= from && x.RatingDate <= asOf)
            .ToListAsync(ct);

        return MapCommitteeFromDays(listId, asOf, rows.Select(x =>
            (x.RatingDate, x.Score, x.Note, x.RatedByUserName, x.RatedAtUtc)));
    }

    private async Task<DelegateFollowupCommitteeRatingDto> LoadSalesDelegateCommitteeAsync(
        int listId,
        DateOnly from,
        DateOnly asOf,
        CancellationToken ct)
    {
        var rows = await _db.SalesEmployeeListDayRatings.AsNoTracking()
            .Where(x => x.OrderListId == listId && x.RatingDate >= from && x.RatingDate <= asOf)
            .ToListAsync(ct);

        var byDay = rows
            .GroupBy(x => x.RatingDate)
            .Select(g => new
            {
                Date = g.Key,
                Score = (int)Math.Round(g.Average(x => x.Score), MidpointRounding.AwayFromZero),
                Note = g.OrderByDescending(x => x.RatedAtUtc).Select(x => x.Note).FirstOrDefault(),
                At = g.Max(x => x.RatedAtUtc)
            })
            .ToList();

        return MapCommitteeFromDays(listId, asOf, byDay.Select(x =>
            (x.Date, x.Score, x.Note, (string?)null, x.At)));
    }

    private static DelegateFollowupCommitteeRatingDto MapCommitteeFromDays(
        int listId,
        DateOnly asOf,
        IEnumerable<(DateOnly Date, int Score, string? Note, string? RatedBy, DateTimeOffset At)> days)
    {
        var material = days
            .Select(x => new DelegateFollowupDayRatingDto
            {
                Date = x.Date,
                Score = x.Score,
                Label = $"{x.Score}/10",
                BoardOperatorScore = x.Score,
                BoardOperatorNote = x.Note
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        var latestDay = DelegateFollowupRules.PickLatestDayRating(material, x => x.Date, asOf);
        DelegateFollowupBoardRatingDto? latest = null;
        if (latestDay is not null)
        {
            var meta = days.First(d => d.Date == latestDay.Date);
            latest = new DelegateFollowupBoardRatingDto
            {
                ListId = listId,
                RatingDate = latestDay.Date,
                Score = latestDay.BoardOperatorScore,
                Note = latestDay.BoardOperatorNote,
                RatedByUserName = meta.RatedBy,
                RatedAtUtc = meta.At,
                HasRating = true
            };
        }

        return new DelegateFollowupCommitteeRatingDto
        {
            Available = true,
            Note = material.Count == 0 ? "لا تقييمات مسجّلة بعد" : "",
            Latest = latest,
            Last30Days = material
        };
    }

    private static RatingSnapshot TakeSnapshot(IReadOnlyList<CustomerFact> facts, DateOnly day)
    {
        var excellent = 0;
        var good = 0;
        var weak = 0;
        var lagging = 0;
        var bad = 0;
        var total = 0;
        var count = 0;

        foreach (var fact in facts)
        {
            var score = ScoreAt(fact, day);
            if (score is null)
                continue;

            count++;
            total += score.Value;
            switch (CustomerRatingCalculator.LevelName(score.Value))
            {
                case DelegateFollowupRules.LabelExcellent: excellent++; break;
                case DelegateFollowupRules.LabelGood: good++; break;
                case DelegateFollowupRules.LabelWeak: weak++; break;
                case DelegateFollowupRules.LabelLagging: lagging++; break;
                default: bad++; break;
            }
        }

        var average = count == 0 ? 0 : (double)total / count;
        return new RatingSnapshot
        {
            CustomerCount = count,
            AverageScore = Math.Round(average, 2, MidpointRounding.AwayFromZero),
            Label = DelegateFollowupRules.ClassifyAverageLabel(average),
            ExcellentCount = excellent,
            GoodCount = good,
            WeakCount = weak,
            LaggingCount = lagging,
            BadCount = bad
        };
    }

    private static string FormatArrivalDetail(IReadOnlyList<(string From, int Count)> breakdown)
    {
        if (breakdown.Count == 0)
            return "لا متحولين اليوم";
        return string.Join(" · ", breakdown.Select(x => $"من {x.From}: {x.Count}"));
    }

    private sealed class ListMeta
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int BranchId { get; set; }
        public string? DelegateName { get; set; }
        public string? FollowerName { get; set; }
    }

    private sealed class OrderRow
    {
        public int OrderListId { get; set; }
        public int CustomerId { get; set; }
        public double SellAmount { get; set; }
        public double DailyInstallmentAmount { get; set; }
        public DateOnly? SaleDate { get; set; }
        public double Receipts { get; set; }
        public int PaymentCount { get; set; }
        public DateOnly? LastPaymentDate { get; set; }
        public double? LastPaymentAmount { get; set; }
        public string? SellerName { get; set; }
        public List<string?> ItemNames { get; set; } = [];
    }

    private sealed class PaymentDayRow
    {
        public int CustomerId { get; set; }
        public DateOnly Date { get; set; }
        public double Amount { get; set; }
    }

    private sealed class CustomerInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ShopName { get; set; }
        public bool IsLegal { get; set; }
    }

    private sealed class CustomerFact
    {
        public int CustomerId { get; set; }
        public double Total { get; set; }
        public double Receipts { get; set; }
        public double Remaining => Total - Receipts;
        public double AmountDaySales { get; set; }
        public int PaymentCount { get; set; }
        public DateOnly? SaleDate { get; set; }
        public DateOnly? LastPaymentDate { get; set; }
        public double? LastPaymentAmount { get; set; }
        public bool IsLegal { get; set; }
    }

    private sealed class RatingSnapshot
    {
        public int CustomerCount { get; set; }
        public double AverageScore { get; set; }
        public required string Label { get; set; }
        public int ExcellentCount { get; set; }
        public int GoodCount { get; set; }
        public int WeakCount { get; set; }
        public int LaggingCount { get; set; }
        public int BadCount { get; set; }
    }
}
