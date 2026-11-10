using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetCollectionLists;

public class GetCommitteeCollectionListsHandler
    : IRequestHandler<GetCommitteeCollectionListsQuery, Result<List<CommitteeCollectionListDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeCollectionListsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteeCollectionListDto>>> Handle(
        GetCommitteeCollectionListsQuery request,
        CancellationToken cancellationToken)
    {
        // قوائم جباية المتابع = مفعّلة + لديها طلبات قيد التحصيل
        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == request.UserId)
            .Where(x => x.FollowerCollectionEnabled)
            .Where(x => x.Orders.Any(o => o.ExecutionStatus == OrderExecutionStatus.InProgress))
            .Select(x => new CommitteeCollectionListDto
            {
                ListId = x.Id,
                ListName = x.Name,
                MandobName = x.Mandob!.FullName,
                ActiveCustomers = x.Orders.Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
            })
            .ToListAsync(cancellationToken);

        return lists;
    }
}
