using Application.Common.Interfaces;
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
                // FollowerCollectionEnabled على القائمة — يوم جباية المتابع / قفل المندوب
                CollectionEnabled = x.FollowerCollectionEnabled
            })
            .ToListAsync(cancellationToken);

        // قائمة فارغة ليست خطأ — المسؤول قد يُربط لاحقاً بأكثر من قائمة.
        return lists;
    }
}
