using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.PostDayRating;

public class GetCommitteeMissingDayRatingsHandler
    : IRequestHandler<GetCommitteeMissingDayRatingsQuery, Result<List<CommitteeMissingDayRatingDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeMissingDayRatingsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteeMissingDayRatingDto>>> Handle(
        GetCommitteeMissingDayRatingsQuery request,
        CancellationToken cancellationToken)
    {
        // قوائم مفعّلة للجباية تحتاج تقييم مندوب قبل إنهاء الدوام
        var lists = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == request.UserId || x.MandobId == request.UserId)
            .Where(x => x.Orders.Any(o => o.ExecutionStatus == OrderExecutionStatus.InProgress))
            .Select(x => new CommitteeMissingDayRatingDto
            {
                ListId = x.Id,
                ListName = x.Name
            })
            .ToListAsync(cancellationToken);

        return lists;
    }
}
