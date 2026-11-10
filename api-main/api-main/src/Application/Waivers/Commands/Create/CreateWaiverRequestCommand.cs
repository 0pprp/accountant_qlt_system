using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.Waivers.Common;
using Domain.Entities.CustomerDecisionAggregate;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.WaiverAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Waivers.Commands.Create;

public sealed class CreateWaiverRequestCommand : IRequest<Result<WaiverDetailDto>>
{
    public required int CustomerId { get; init; }
    public required string Note { get; init; }
    public int? OrderId { get; init; }
}

public sealed class CreateWaiverRequestHandler
    : IRequestHandler<CreateWaiverRequestCommand, Result<WaiverDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreateWaiverRequestHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WaiverDetailDto>> Handle(CreateWaiverRequestCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Create) == false)
            return WaiverErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WaiverErrors.Forbidden;

        var note = (request.Note ?? "").Trim();
        if (note.Length < 1)
            return WaiverErrors.NoteRequired;
        if (note.Length > 2000)
            return WaiverErrors.NoteTooLong;

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
        if (customer is null)
            return WaiverErrors.CustomerNotFound;

        var branchIds = _current.BranchIds ?? [];
        if (branchIds.Count > 0 && branchIds.Contains(customer.BranchId) == false)
            return WaiverErrors.BranchDenied;

        if (request.OrderId is int oid)
        {
            var orderOk = await _db.Orders.AsNoTracking()
                .AnyAsync(o => o.Id == oid && o.CustomerId == customer.Id, ct);
            if (orderOk == false)
                return WaiverErrors.NotFound;
        }

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var entity = new CustomerWaiverRequest(
            customer.Id, customer.BranchId, request.OrderId, actorId, actorName, note, at);
        _db.CustomerWaiverRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        _db.CustomerDecisions.Add(new CustomerDecision(
            customer.Id, customer.BranchId, actorId, actorName,
            CustomerDecisionType.WaiverRequest, note,
            null, null, null, null, entity.Id, null, at));
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var branchName = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == customer.BranchId).Select(b => b.Name).FirstOrDefaultAsync(ct);
        return WaiverMapper.ToDetail(entity, customer.FullName, customer.PhoneNumber, branchName, null, null, null);
    }
}
