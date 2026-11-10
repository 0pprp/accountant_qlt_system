using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetPendingSales;

public class GetCommitteePendingSalesHandler
    : IRequestHandler<GetCommitteePendingSalesQuery, Result<List<CommitteePendingSaleDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteePendingSalesHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteePendingSaleDto>>> Handle(
        GetCommitteePendingSalesQuery request,
        CancellationToken cancellationToken)
    {
        // معلّقات المصادقة: قوائم Motaba فقط، بلا سجل مصادقة، ولم يبدأ التنفيذ
        var authenticatedOrderIds = _db.FollowerSaleAuthentications.AsNoTracking()
            .Select(a => a.OrderId);

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderList != null
                        && o.OrderList.MotabaId == request.UserId
                        && o.ExecutionStatus == OrderExecutionStatus.NotStarted
                        && authenticatedOrderIds.Contains(o.Id) == false)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new CommitteePendingSaleDto
            {
                OrderId = o.Id,
                ListId = o.OrderListId,
                CustomerName = o.Customer!.FullName,
                SellAmount = o.SellAmount,
                Status = "pending_auth",
                CreatedAt = o.CreatedAt
            })
            .Take(100)
            .ToListAsync(cancellationToken);

        return rows;
    }
}
