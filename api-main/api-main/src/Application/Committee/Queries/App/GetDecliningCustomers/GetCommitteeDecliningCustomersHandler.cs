using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetDecliningCustomers;

public class GetCommitteeDecliningCustomersHandler
    : IRequestHandler<GetCommitteeDecliningCustomersQuery, Result<List<CommitteeDecliningCustomerDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetCommitteeDecliningCustomersHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<List<CommitteeDecliningCustomerDto>>> Handle(
        GetCommitteeDecliningCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var from = today.AddDays(-14);

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderList != null
                        && (o.OrderList.MotabaId == request.UserId || o.OrderList.MandobId == request.UserId)
                        && o.ExecutionStatus == OrderExecutionStatus.InProgress)
            .Select(o => new
            {
                o.CustomerId,
                ListId = o.OrderListId!.Value,
                ListName = o.OrderList!.Name,
                FullName = o.Customer!.FullName,
                Phone = o.Customer.PhoneNumber,
                o.DailyInstallmentAmount,
                PaidDays = o.InstallmentPayments.Count(ip => ip.Date >= from && ip.Date <= today && ip.Amount > 0)
            })
            .ToListAsync(cancellationToken);

        var expected = 14;
        return rows
            .Select(r =>
            {
                var missed = Math.Max(0, expected - r.PaidDays);
                return new CommitteeDecliningCustomerDto
                {
                    CustomerId = r.CustomerId,
                    ListId = r.ListId,
                    ListName = r.ListName,
                    FullName = r.FullName,
                    Phone = r.Phone,
                    MissedDays = missed,
                    RiskLabel = missed >= 7 ? "سيء" : missed >= 4 ? "متلكئ" : "ضعيف"
                };
            })
            .Where(x => x.MissedDays >= 3)
            .OrderByDescending(x => x.MissedDays)
            .ToList();
    }
}
