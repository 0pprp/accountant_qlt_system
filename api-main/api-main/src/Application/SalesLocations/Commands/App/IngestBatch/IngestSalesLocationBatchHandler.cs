using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesLocations.Common;
using Domain.Entities.SalesLocationAggregate;

namespace Application.SalesLocations.Commands.App.IngestBatch;

public class IngestSalesLocationBatchHandler
    : IRequestHandler<IngestSalesLocationBatchCommand, Result<IngestSalesLocationBatchResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IngestSalesLocationBatchHandler(
        IApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IngestSalesLocationBatchResponse>> Handle(
        IngestSalesLocationBatchCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Points.Count > FollowerLocationRules.MaxBatchPoints)
            return SalesLocationErrors.BatchTooLarge;

        var (shift, shiftErr) = await SalesLocationIngestHelper.RequireActiveShiftAsync(
            _dbContext, request.UserId, cancellationToken);
        if (shiftErr is not null)
            return shiftErr.Value;

        var receivedAt = _dateTimeProvider.UtcNowOffset;
        var workforceErr = await SalesLocationIngestHelper.RequireSalesWorkforceIfNeededAsync(
            _dbContext, request.UserId, shift!.BranchId, request.ActorKind, receivedAt, cancellationToken);
        if (workforceErr is not null)
            return workforceErr.Value;

        var accepted = 0;
        var duplicates = 0;
        var rejected = 0;

        foreach (var dto in request.Points)
        {
            if (FollowerLocationRules.IsValidCoordinate(dto.Lat, dto.Lng) == false)
            {
                rejected++;
                continue;
            }

            var capturedAt = dto.RecordedAtUtc == default ? receivedAt : dto.RecordedAtUtc;
            if (SalesLocationIngestHelper.ValidateCaptureTime(capturedAt, receivedAt, shift.StartedAtUtc) is not null)
            {
                rejected++;
                continue;
            }

            var clientEventId = SalesLocationIngestHelper.NormalizeClientEventId(dto.ClientEventId);
            if (await SalesLocationIngestHelper.IsDuplicateAsync(
                    _dbContext, request.UserId, clientEventId, cancellationToken))
            {
                duplicates++;
                continue;
            }

            var accuracy = dto.AccuracyMeters;
            if (accuracy is double a && (double.IsFinite(a) == false || a < 0))
                accuracy = null;

            _dbContext.SalesLocationPoints.Add(new SalesLocationPoint(
                request.UserId,
                shift.BranchId,
                dto.Lat,
                dto.Lng,
                capturedAt,
                receivedAt,
                isLive: false,
                request.ActorKind,
                accuracy,
                shift.Id,
                clientEventId));
            accepted++;
        }

        if (accepted > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return new IngestSalesLocationBatchResponse
        {
            AcceptedCount = accepted,
            DuplicateCount = duplicates,
            RejectedCount = rejected
        };
    }
}
