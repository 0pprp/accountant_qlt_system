using Application.Common.Interfaces;
using Domain.Entities.SalesLocationAggregate;

namespace Application.SalesLocations.Commands.App.IngestBatch;

public class IngestSalesLocationBatchHandler : IRequestHandler<IngestSalesLocationBatchCommand, Result<IngestSalesLocationBatchResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public IngestSalesLocationBatchHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IngestSalesLocationBatchResponse>> Handle(IngestSalesLocationBatchCommand request, CancellationToken cancellationToken)
    {
        var branchId = _currentUserService.BranchIds?.FirstOrDefault();

        foreach (var point in request.Points)
        {
            _dbContext.SalesLocationPoints.Add(new SalesLocationPoint(
                request.UserId,
                branchId,
                point.Lat,
                point.Lng,
                point.RecordedAtUtc,
                isLive: false));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IngestSalesLocationBatchResponse { AcceptedCount = request.Points.Count };
    }
}
