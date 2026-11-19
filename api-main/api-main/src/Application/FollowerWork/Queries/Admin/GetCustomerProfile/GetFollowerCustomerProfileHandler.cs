using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.ManagementSummary.Common;
using Application.SalesRatings.Common;
using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Queries.Admin.GetCustomerProfile;

public sealed class GetFollowerCustomerProfileHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    FollowerSalesReviewDataService scopeService)
    : IRequestHandler<GetFollowerCustomerProfileQuery, Result<GetFollowerCustomerProfileResponse>>
{
    public async Task<Result<GetFollowerCustomerProfileResponse>> Handle(
        GetFollowerCustomerProfileQuery request,
        CancellationToken ct)
    {
        if (FollowerWorkAccess.CanReadDirectory(currentUser) == false)
            return FollowerWorkErrors.AccessDenied;

        var branchErr = FollowerWorkAccess.ValidateBranchOrAll(currentUser, request.BranchId);
        if (branchErr is not null)
            return branchErr.Value;

        var branches = FollowerWorkAccess.ResolveBranchScope(currentUser, request.BranchId);
        var scope = await scopeService.ResolveFollowerScopeAsync(request.FollowerId, branches, request.BranchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var listIds = scope.Data!.ListIds;
        var orders = await db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == request.CustomerId
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value))
            .OrderByDescending(o => o.SaleDate)
            .ThenByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                ListId = o.OrderListId!.Value,
                ListName = o.OrderList != null ? o.OrderList.Name : null,
                SellerName = o.Seller != null ? o.Seller.FullName : null
            })
            .ToListAsync(ct);
        if (orders.Count == 0)
            return FollowerWorkErrors.CustomerOutOfScope;

        var customer = await db.Customers.AsNoTracking()
            .Where(c => c.Id == request.CustomerId)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.PhoneNumber,
                ShopName = c.Business.Name,
                c.Business.Address,
                c.IsLegal,
                Lat = c.Business.GpsLatitude,
                Lng = c.Business.GpsLongitude
            })
            .FirstOrDefaultAsync(ct);
        if (customer is null)
            return FollowerWorkErrors.CustomerOutOfScope;

        var asOf = IraqCalendar.ToLocalDate(clock.UtcNowOffset);
        var facts = await FollowerFactsQuery.LoadAsync(db, listIds, null, [request.CustomerId], ct);
        var fact = facts.OrderByDescending(f => f.AmountRemaining).FirstOrDefault();
        string? ratingLabel = null;
        if (fact is not null)
        {
            var score = CustomerRatingCalculator.ScoreFor(
                fact.AmountTotalSales, fact.ReceiptsTotal, fact.AmountRemaining,
                fact.SaleDate, fact.LastPaymentDate, asOf, usePhantomSaleDate: true);
            if (score is not null)
                ratingLabel = CustomerRatingCalculator.LevelName(score.Value);
        }

        var latestOrder = orders[0];
        var general = new FollowerCustomerGeneralDto(
            customer.Id,
            customer.FullName,
            customer.PhoneNumber,
            customer.ShopName,
            customer.Address,
            latestOrder.ListId,
            latestOrder.ListName,
            customer.IsLegal,
            customer.IsLegal ? "قانونية" : ratingLabel,
            fact?.AmountTotalSales ?? 0,
            fact?.SaleDate,
            fact?.AmountDaySales ?? 0,
            latestOrder.SellerName,
            fact?.ReceiptsTotal ?? 0,
            fact?.AmountRemaining ?? 0,
            fact?.LastPaymentDate,
            customer.Lat is not null && customer.Lng is not null,
            customer.Lat,
            customer.Lng);

        var orderIds = orders.Select(o => o.Id).ToList();
        var auth = await db.FollowerSaleAuthentications.AsNoTracking()
            .Where(a => orderIds.Contains(a.OrderId))
            .OrderByDescending(a => a.AuthenticatedAtUtc)
            .Select(a => new FollowerCustomerAssessmentDto(
                a.Rating,
                a.Note,
                (double)a.ShopStockEstimatedValue,
                (double)a.EstimatedDailyRevenue,
                a.FollowerName,
                a.AuthenticatedAtUtc,
                a.Rating >= 1 && a.Rating <= FollowerSaleAuthRules.WrongSaleMaxRating))
            .FirstOrDefaultAsync(ct);

        var staff = await db.CustomerStaffRatings.AsNoTracking()
            .Where(r => r.CustomerId == request.CustomerId
                        && (r.Role == CustomerStaffRating.RoleDelegate || r.Role == CustomerStaffRating.RoleSalesEmployee))
            .OrderByDescending(r => r.RatedAtUtc)
            .Select(r => new
            {
                r.Role,
                r.Score,
                r.Note,
                r.RatedByName,
                r.RatedAtUtc,
                r.ShopStockEstimatedValue,
                r.EstimatedDailyRevenue
            })
            .ToListAsync(ct);

        var delegateRow = staff.FirstOrDefault(r => r.Role == CustomerStaffRating.RoleDelegate);
        var employeeRow = staff.FirstOrDefault(r => r.Role == CustomerStaffRating.RoleSalesEmployee);

        var shopProfile = await db.CustomerShopProfiles.AsNoTracking()
            .Where(p => p.CustomerId == request.CustomerId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ThenByDescending(p => p.Id)
            .Select(p => new { p.ShopName, p.BusinessType, p.StockEstimatedValue, p.EstimatedDailyRevenue, p.ShopArea })
            .FirstOrDefaultAsync(ct);

        var delegateAssessment = delegateRow is null
            ? null
            : new FollowerCustomerAssessmentDto(
                delegateRow.Score,
                delegateRow.Note,
                delegateRow.ShopStockEstimatedValue,
                delegateRow.EstimatedDailyRevenue,
                delegateRow.RatedByName,
                delegateRow.RatedAtUtc,
                delegateRow.Score <= FollowerSaleAuthRules.WrongSaleMaxRating);

        var employee = employeeRow is null && shopProfile is null
            ? null
            : new FollowerCustomerSalesEmployeeDto(
                employeeRow?.Score,
                employeeRow?.Note,
                employeeRow?.RatedByName,
                employeeRow?.RatedAtUtc,
                shopProfile?.ShopName,
                shopProfile?.BusinessType,
                shopProfile?.StockEstimatedValue ?? employeeRow?.ShopStockEstimatedValue,
                shopProfile?.EstimatedDailyRevenue ?? employeeRow?.EstimatedDailyRevenue,
                shopProfile?.ShopArea);

        return new GetFollowerCustomerProfileResponse(
            general,
            auth,
            delegateAssessment,
            employee,
            asOf.ToString("yyyy-MM-dd"));
    }
}
