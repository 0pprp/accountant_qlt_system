using Application.Common.Interfaces;
using Application.Committee.Common;
using Domain.Entities.FollowerWorkAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetDaily;

public class GetCommitteeDailyHandler : IRequestHandler<GetCommitteeDailyQuery, Result<CommitteeDailyResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeDailyHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<CommitteeDailyResponse>> Handle(
        GetCommitteeDailyQuery request,
        CancellationToken cancellationToken)
    {
        if (!await CommitteeAccess.OwnsListAsync(_db, request.UserId, request.ListId, cancellationToken))
            return CommitteeErrors.ListNotFound;

        var list = await _db.OrderLists.AsNoTracking()
            .Where(x => x.Id == request.ListId)
            .Select(x => new { x.Id, x.Name })
            .FirstAsync(cancellationToken);

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId == request.ListId && o.ExecutionStatus == OrderExecutionStatus.InProgress)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                FullName = o.Customer!.FullName,
                Phone = o.Customer.PhoneNumber,
                o.DailyInstallmentAmount,
                PaidToday = o.InstallmentPayments.Where(ip => ip.Date == request.Date).Sum(ip => ip.Amount),
                CustomerHasGps = o.Customer.Business.GpsLatitude != null
                                 && o.Customer.Business.GpsLongitude != null
                                 && !(o.Customer.Business.GpsLatitude == 0
                                      && o.Customer.Business.GpsLongitude == 0),
                OrderHasGps = o.Location != null
                              && !(o.Location.Latitude == 0 && o.Location.Longitude == 0)
            })
            .ToListAsync(cancellationToken);

        var customerIds = rows.Select(r => r.CustomerId).Distinct().ToList();
        var notedIds = customerIds.Count == 0
            ? new HashSet<int>()
            : (await _db.CustomerNotes.AsNoTracking()
                .Where(n => customerIds.Contains(n.CustomerId)
                            && (n.OrderListId == null || n.OrderListId == request.ListId))
                .Select(n => n.CustomerId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var latestNotes = customerIds.Count == 0
            ? new Dictionary<int, string?>()
            : (await _db.FollowerFieldFollowups.AsNoTracking()
                .Where(f =>
                    f.FollowerUserId == request.UserId
                    && f.ListId == request.ListId
                    && f.Status == FollowerFieldFollowup.StatusCompleted
                    && f.CompletionNote != null
                    && customerIds.Contains(f.CustomerId))
                .OrderByDescending(f => f.CompletedAtUtc)
                .ThenByDescending(f => f.Id)
                .Select(f => new { f.CustomerId, f.CompletionNote })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.CustomerId)
            .ToDictionary(g => g.Key, g => g.First().CompletionNote);

        var byCustomer = rows
            .GroupBy(r => r.CustomerId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();
                    var paidToday = g.Sum(x => x.PaidToday);
                    var installment = g.Sum(x => x.DailyInstallmentAmount);
                    var activeOrderId = g.OrderByDescending(x => x.Id).First().Id;
                    var hasLocation = g.Any(x => x.CustomerHasGps || x.OrderHasGps);
                    return new
                    {
                        first.FullName,
                        first.Phone,
                        PaidToday = paidToday,
                        DailyInstallment = installment,
                        ActiveOrderId = activeOrderId,
                        HasLocation = hasLocation
                    };
                });

        var showPaid = request.ShowType.Contains("مسددين", StringComparison.Ordinal)
                       && !request.ShowType.Contains("الغير", StringComparison.Ordinal);

        var customers = byCustomer
            .Select(kv => new CommitteeDailyCustomerDto
            {
                CustomerId = kv.Key,
                FullName = kv.Value.FullName,
                Phone = kv.Value.Phone,
                DailyInstallment = kv.Value.DailyInstallment,
                PaidToday = kv.Value.PaidToday,
                IsPaid = kv.Value.PaidToday >= kv.Value.DailyInstallment && kv.Value.DailyInstallment > 0,
                HasNote = notedIds.Contains(kv.Key),
                LatestFieldFollowupNote = latestNotes.GetValueOrDefault(kv.Key),
                HasLocation = kv.Value.HasLocation,
                ActiveOrderId = kv.Value.ActiveOrderId
            })
            .Where(c => showPaid ? c.IsPaid : !c.IsPaid)
            .OrderBy(c => c.FullName)
            .ToList();

        return new CommitteeDailyResponse
        {
            ListId = list.Id,
            ListName = list.Name,
            ShowType = request.ShowType,
            Date = request.Date,
            Customers = customers
        };
    }
}
