using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesLocations.Common;
using Domain.Entities.SalesLocationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesLocations.Commands.App.IngestLive;

public class IngestSalesLiveLocationHandler
    : IRequestHandler<IngestSalesLiveLocationCommand, Result<IngestSalesLiveLocationResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IngestSalesLiveLocationHandler(
        IApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IngestSalesLiveLocationResponse>> Handle(
        IngestSalesLiveLocationCommand request,
        CancellationToken cancellationToken)
    {
        if (FollowerLocationRules.IsValidCoordinate(request.Lat, request.Lng) == false)
            return SalesLocationErrors.InvalidCoordinates;

        var (shift, shiftErr) = await SalesLocationIngestHelper.RequireActiveShiftAsync(
            _dbContext, request.UserId, cancellationToken);
        if (shiftErr is not null)
            return shiftErr.Value;

        var receivedAt = _dateTimeProvider.UtcNowOffset;
        var workforceErr = await SalesLocationIngestHelper.RequireSalesWorkforceIfNeededAsync(
            _dbContext, request.UserId, shift!.BranchId, request.ActorKind, receivedAt, cancellationToken);
        if (workforceErr is not null)
            return workforceErr.Value;

        var capturedAt = request.CapturedAtUtc ?? receivedAt;
        var timeErr = SalesLocationIngestHelper.ValidateCaptureTime(capturedAt, receivedAt, shift.StartedAtUtc);
        if (timeErr is not null)
            return timeErr.Value;

        var clientEventId = SalesLocationIngestHelper.NormalizeClientEventId(request.ClientEventId);
        if (await SalesLocationIngestHelper.IsDuplicateAsync(
                _dbContext, request.UserId, clientEventId, cancellationToken))
        {
            var existing = await _dbContext.SalesLocationPoints.AsNoTracking()
                .Where(p => p.UserId == request.UserId && p.ClientEventId == clientEventId)
                .OrderByDescending(p => p.Id)
                .FirstAsync(cancellationToken);
            return new IngestSalesLiveLocationResponse
            {
                Id = existing.Id,
                Lat = existing.Latitude,
                Lng = existing.Longitude,
                AccuracyMeters = existing.AccuracyMeters,
                CapturedAtUtc = existing.RecordedAtUtc,
                ReceivedAtUtc = existing.ReceivedAtUtc,
                Duplicate = true
            };
        }

        var accuracy = request.AccuracyMeters;
        if (accuracy is double a && (double.IsFinite(a) == false || a < 0))
            accuracy = null;

        var point = new SalesLocationPoint(
            request.UserId,
            shift.BranchId,
            request.Lat,
            request.Lng,
            capturedAt,
            receivedAt,
            isLive: true,
            request.ActorKind,
            accuracy,
            shift.Id,
            clientEventId);

        _dbContext.SalesLocationPoints.Add(point);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IngestSalesLiveLocationResponse
        {
            Id = point.Id,
            Lat = point.Latitude,
            Lng = point.Longitude,
            AccuracyMeters = point.AccuracyMeters,
            CapturedAtUtc = point.RecordedAtUtc,
            ReceivedAtUtc = point.ReceivedAtUtc,
            Duplicate = false
        };
    }
}
