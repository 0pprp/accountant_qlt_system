using Application.Common.Interfaces;
using Application.Committee.Common;
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
                o.CustomerId,
                FullName = o.Customer!.FullName,
                Phone = o.Customer.PhoneNumber,
                o.DailyInstallmentAmount,
                PaidToday = o.InstallmentPayments.Where(ip => ip.Date == request.Date).Sum(ip => ip.Amount)
            })
            .ToListAsync(cancellationToken);

        var showPaid = request.ShowType.Contains("مسددين", StringComparison.Ordinal)
                       && !request.ShowType.Contains("الغير", StringComparison.Ordinal);

        var customers = rows
            .Select(r => new CommitteeDailyCustomerDto
            {
                CustomerId = r.CustomerId,
                FullName = r.FullName,
                Phone = r.Phone,
                DailyInstallment = r.DailyInstallmentAmount,
                PaidToday = r.PaidToday,
                IsPaid = r.PaidToday >= r.DailyInstallmentAmount && r.DailyInstallmentAmount > 0
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
