using Application.Common.Interfaces;
using Domain.Entities.SalesLocationAggregate;

namespace Application.SalesLocations.Commands.App.IngestLive;

public class IngestSalesLiveLocationHandler : IRequestHandler<IngestSalesLiveLocationCommand, Result<IngestSalesLiveLocationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IngestSalesLiveLocationHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IngestSalesLiveLocationResponse>> Handle(IngestSalesLiveLocationCommand request, CancellationToken cancellationToken)
    {
        var recordedAt = _dateTimeProvider.UtcNowOffset;
        var branchId = _currentUserService.BranchIds?.FirstOrDefault();

        var point = new SalesLocationPoint(
            request.UserId,
            branchId,
            request.Lat,
            request.Lng,
            recordedAt,
            isLive: true);

        _dbContext.SalesLocationPoints.Add(point);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IngestSalesLiveLocationResponse
        {
            Id = point.Id,
            Lat = point.Latitude,
            Lng = point.Longitude,
            RecordedAtUtc = point.RecordedAtUtc
        };
    }
}