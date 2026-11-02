using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetLists;

public class GetCommitteeListsHandler : IRequestHandler<GetCommitteeListsQuery, Result<List<CommitteeListDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeListsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteeListDto>>> Handle(GetCommitteeListsQuery request, CancellationToken cancellationToken)
    {
        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == request.UserId || x.MandobId == request.UserId)
            .Select(x => new CommitteeListDto
            {
                Id = x.Id,
                Name = x.Name,
                MandobName = x.Mandob!.FullName,
                BranchId = x.BranchId,
                // الجباية مفعّلة تشغيلياً عند وجود طلبات قيد التحصيل في القائمة
                CollectionEnabled = x.Orders.Any(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
            })
            .ToListAsync(cancellationToken);

        // قائمة فارغة ليست خطأ — المسؤول قد يُربط لاحقاً بأكثر من قائمة.
        return lists;
    }
}
