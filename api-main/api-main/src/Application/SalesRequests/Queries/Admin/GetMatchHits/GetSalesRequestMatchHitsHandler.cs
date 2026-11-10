using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.SeedData;
using Application.Customers.Common;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Queries.Admin.GetMatchHits;

public class GetSalesRequestMatchHitsHandler
    : IRequestHandler<GetSalesRequestMatchHitsQuery, Result<PaginatedList<GetSalesRequestMatchHitItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetSalesRequestMatchHitsHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<GetSalesRequestMatchHitItem>>> Handle(
        GetSalesRequestMatchHitsQuery request,
        CancellationToken cancellationToken)
    {
        if (!SalesMatchCriteria.All.Contains(request.Criterion))
            return SalesRequestErrors.InvalidCriterion;

        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var salesRequest = await _db.SalesRequests.AsNoTracking()
            .Where(x => x.Id == request.RequestId && !x.IsDeleted)
            .Select(x => new { x.Id, x.CustomerName, x.CustomerPhone, x.BranchId })
            .FirstOrDefaultAsync(cancellationToken);

        if (salesRequest is null || !branches.Contains(salesRequest.BranchId))
            return SalesRequestErrors.NotFound;

        var card = SalesRequestMatchEvaluator.EvaluateCard(
            request.Criterion,
            salesRequest.CustomerName,
            salesRequest.CustomerPhone,
            []);

        // بيانات ناقصة أو فشل: لا نرجع count=0 كنجاح
        if (card.State is "insufficientData" or "failed")
        {
            return new PaginatedList<GetSalesRequestMatchHitItem>(
                [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);
        }

        var customers = await SalesMatchCandidateSelector.LoadCustomerCandidatesAsync(
            _db,
            branches,
            request.Criterion,
            salesRequest.CustomerName,
            salesRequest.CustomerPhone,
            cancellationToken);

        var hitIds = SalesRequestMatchEvaluator
            .FilterHits(request.Criterion, salesRequest.CustomerName, salesRequest.CustomerPhone, customers)
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        var total = hitIds.Count;
        var pageIds = hitIds
            .Skip((request.Pagination.PageIndex - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToList();

        if (pageIds.Count == 0)
        {
            return new PaginatedList<GetSalesRequestMatchHitItem>(
                [], total, request.Pagination.PageIndex, request.Pagination.PageSize);
        }

        var canReadFinancial = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadFinancial);

        var asOf = _clock.Today;
        var rows = await _db.Customers.AsNoTracking()
            .Where(x => pageIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.PhoneNumber,
                BranchName = x.Branch!.Name,
                SellAmount = x.Orders
                    .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .Sum(o => o.SellAmount),
                PaidAmount = x.Orders
                    .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .SelectMany(o => o.InstallmentPayments)
                    .Sum(ip => ip.Amount),
                PaymentCount = x.Orders
                    .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .SelectMany(o => o.InstallmentPayments)
                    .Count(),
                SaleDate = x.Orders
                    .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved && o.SaleDate != null)
                    .Select(o => o.SaleDate)
                    .Min(),
                LastPayment = x.Orders
                    .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved)
                    .SelectMany(o => o.InstallmentPayments)
                    .Select(ip => (DateOnly?)ip.Date)
                    .Max()
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row =>
        {
            var remaining = row.SellAmount - row.PaidAmount;
            var rating = CustomerRatingCalculator.EvaluateLabel(
                row.SellAmount, row.PaidAmount, remaining, row.SaleDate, row.LastPayment, asOf);
            var paymentDays = row.SaleDate is { } sd && row.LastPayment is { } lp
                ? CustomerRatingCalculator.DiffDaysInclusive(
                    CustomerRatingCalculator.AdjustSaleDate(sd), lp)
                : (int?)null;

            // IsLegal غير موجود في نموذج الزبون الحالي — يُرجع null (متوقف على المخطط)
            double? sell = canReadFinancial ? row.SellAmount : null;
            double? paid = canReadFinancial ? row.PaidAmount : null;
            double? rem = canReadFinancial ? remaining : null;
            int? payCount = canReadFinancial ? row.PaymentCount : null;
            int? payDays = canReadFinancial ? paymentDays : null;

            return new GetSalesRequestMatchHitItem
            {
                CustomerId = row.Id,
                CustomerName = row.FullName,
                MatchReason = SalesRequestMatchEvaluator.BuildMatchReason(
                    request.Criterion, salesRequest.CustomerName, row.FullName),
                HighlightedParts = BuildHighlights(request.Criterion, salesRequest.CustomerName, row.FullName),
                Phone = row.PhoneNumber,
                BranchName = row.BranchName,
                RatingLabel = rating,
                Score = null,
                IsLegal = null,
                SellTotal = sell,
                PaidTotal = paid,
                Remaining = rem,
                PaymentCount = payCount,
                PaymentDays = payDays,
                SaleDate = row.SaleDate,
                CanOpenProfile = _currentUser.HasPermission(SeedPermissions.SalesRequest.ReadAuditProfile)
            };
        }).ToList();

        return new PaginatedList<GetSalesRequestMatchHitItem>(
            items, total, request.Pagination.PageIndex, request.Pagination.PageSize);
    }

    private static List<string> BuildHighlights(string criterion, string? query, string candidate)
    {
        var (_, q1, q2) = SalesRequestNameSimilarity.TripleParts(query);
        var (c0, c1, c2) = SalesRequestNameSimilarity.TripleParts(candidate);
        return criterion switch
        {
            SalesMatchCriteria.TripleName => new List<string> { c0, c1, c2 }.Where(x => x.Length > 0).ToList(),
            SalesMatchCriteria.FatherGrandfather => new List<string> { c1, c2 }.Where(x => x.Length > 0).ToList(),
            SalesMatchCriteria.SonFather => new List<string> { q1, q2, c0, c1 }.Where(x => x.Length > 0).Distinct().ToList(),
            _ => []
        };
    }

}
