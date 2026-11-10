using Application.Committee.Common;
using Application.Common.Interfaces;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.FieldFollowups;

public record AddCommitteeFieldFollowupCommand(
    int UserId,
    int ListId,
    string? ListName,
    int CustomerId,
    string? CustomerName,
    string? PhoneNumber) : IRequest<Result<CommitteeFieldFollowupDto>>;

public class AddCommitteeFieldFollowupHandler
    : IRequestHandler<AddCommitteeFieldFollowupCommand, Result<CommitteeFieldFollowupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public AddCommitteeFieldFollowupHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<CommitteeFieldFollowupDto>> Handle(
        AddCommitteeFieldFollowupCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ListId <= 0 || request.CustomerId <= 0)
            return CommitteeFieldFollowupErrors.IncompleteData;

        var list = await _db.OrderLists.AsNoTracking()
            .Where(x => x.Id == request.ListId && x.MotabaId == request.UserId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (list is null)
            return CommitteeErrors.ListNotFound;

        var customerExists = await _db.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (!customerExists)
            return CommitteeFieldFollowupErrors.CustomerNotFound;

        var existing = await _db.FollowerFieldFollowups
            .Where(x =>
                x.FollowerUserId == request.UserId
                && x.ListId == request.ListId
                && x.CustomerId == request.CustomerId
                && x.Status == FollowerFieldFollowup.StatusPending)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return Map(existing);

        var row = new FollowerFieldFollowup(
            request.UserId,
            list.Id,
            string.IsNullOrWhiteSpace(request.ListName) ? list.Name : request.ListName,
            request.CustomerId,
            request.CustomerName,
            request.PhoneNumber,
            _clock.UtcNow);

        _db.FollowerFieldFollowups.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(row);
    }

    internal static CommitteeFieldFollowupDto Map(FollowerFieldFollowup x) => new()
    {
        Id = x.Id,
        FollowerUserId = x.FollowerUserId,
        ListId = x.ListId,
        ListName = x.ListName,
        CustomerId = x.CustomerId,
        CustomerName = x.CustomerName,
        PhoneNumber = x.PhoneNumber,
        Status = x.Status,
        CompletionNote = x.CompletionNote,
        CreatedAtUtc = x.CreatedAtUtc,
        CompletedAtUtc = x.CompletedAtUtc
    };
}
