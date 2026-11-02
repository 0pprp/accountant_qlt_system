using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetListCards;

public class GetCommitteeListCardsHandler
    : IRequestHandler<GetCommitteeListCardsQuery, Result<List<CommitteeListCardDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetCommitteeListCardsHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<List<CommitteeListCardDto>>> Handle(
        GetCommitteeListCardsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var prevMonthEnd = monthStart.AddDays(-1);
        var prevMonthStart = new DateOnly(prevMonthEnd.Year, prevMonthEnd.Month, 1);

        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == request.UserId || x.MandobId == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                MandobName = x.Mandob!.FullName,
                ActiveCustomers = x.Orders.Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress),
                CollectionEnabled = x.Orders.Any(o => o.ExecutionStatus == OrderExecutionStatus.InProgress),
                CurrentPaid = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .SelectMany(o => o.InstallmentPayments)
                    .Count(ip => ip.Date >= monthStart && ip.Date <= today),
                PrevPaid = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .SelectMany(o => o.InstallmentPayments)
                    .Count(ip => ip.Date >= prevMonthStart && ip.Date <= prevMonthEnd),
                ExpectedDays = Math.Max(1, today.Day)
            })
            .ToListAsync(cancellationToken);

        if (lists.Count == 0)
            return new List<CommitteeListCardDto>();

        var cards = lists.Select(x =>
        {
            var cur = Math.Clamp(x.CurrentPaid / (double)(x.ActiveCustomers * x.ExpectedDays + 0.0001) * 10.0, 0, 10);
            var prev = Math.Clamp(x.PrevPaid / (double)(x.ActiveCustomers * 30 + 0.0001) * 10.0, 0, 10);
            return new CommitteeListCardDto
            {
                ListId = x.Id,
                ListName = x.Name,
                MandobName = x.MandobName,
                PhantomLabel = PhantomOf(cur),
                CurrentMonthScore = Math.Round(cur, 1),
                PreviousMonthScore = Math.Round(prev, 1),
                ActiveCustomers = x.ActiveCustomers,
                CollectionEnabled = x.CollectionEnabled
            };
        }).ToList();

        return cards;
    }

    static string PhantomOf(double score) => score switch
    {
        >= 9 => "ممتاز",
        >= 7 => "جيد",
        >= 5 => "ضعيف",
        >= 3 => "متلكئ",
        _ => "سيء"
    };
}
