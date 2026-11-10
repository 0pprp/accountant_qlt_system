using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.CustomerDecisions.Common;
using Application.SalesRequests.Common;
using Application.Waivers.Common;
using Domain.Entities.CustomerDecisionAggregate;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.LegalConversionAggregate;
using Domain.Entities.LegalConversionAggregate.Enums;
using Domain.Entities.WaiverAggregate;
using Domain.Entities.WaiverAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.CustomerDecisions.Commands;

public sealed class PostBranchDecisionCommand : IRequest<Result<PostBranchDecisionResultDto>>
{
    public required int CustomerId { get; init; }
    /// <summary>Continue | FakeSale | LegalRequest | WaiverRequest — أو التسمية العربية.</summary>
    public required string DecisionType { get; init; }
    public required string Note { get; init; }
    public int? OrderId { get; init; }
}

public sealed class PostBranchDecisionHandler
    : IRequestHandler<PostBranchDecisionCommand, Result<PostBranchDecisionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public PostBranchDecisionHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<PostBranchDecisionResultDto>> Handle(
        PostBranchDecisionCommand request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CustomerDecision.Create) == false)
            return CustomerDecisionErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return CustomerDecisionErrors.Forbidden;

        var note = (request.Note ?? "").Trim();
        if (note.Length < 1)
            return CustomerDecisionErrors.NoteRequired;

        if (TryResolveType(request.DecisionType, out var type) == false
            || type is CustomerDecisionType.Legal or CustomerDecisionType.Waiver)
            return CustomerDecisionErrors.InvalidType;

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct);
        if (customer is null)
            return CustomerDecisionErrors.CustomerNotFound;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: customer.BranchId, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        if (type is CustomerDecisionType.Continue or CustomerDecisionType.FakeSale or CustomerDecisionType.LegalRequest)
        {
            if (_current.HasPermission(SeedPermissions.LegalNames.Read)
                && await IsEligibleWeakPayerAsync(customer.Id, customer.IsLegal, customer.IsFakeSale, ct) == false)
                return CustomerDecisionErrors.NotInQueue;
        }

        if (type == CustomerDecisionType.WaiverRequest
            && _current.HasPermission(SeedPermissions.Waiver.Create) == false)
            return WaiverErrors.Forbidden;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;
        int? waiverId = null;
        int? conversionId = null;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        DateTimeOffset? snooze = type == CustomerDecisionType.Continue ? at.AddDays(7) : null;
        if (type == CustomerDecisionType.FakeSale)
            customer.MarkFakeSale();

        if (type == CustomerDecisionType.LegalRequest)
        {
            var conversion = new LegalConversionRequest(
                customer.Id, customer.BranchId, actorId, actorName, note, at);
            _db.LegalConversionRequests.Add(conversion);
            await _db.SaveChangesAsync(ct);
            conversionId = conversion.Id;
        }

        if (type == CustomerDecisionType.WaiverRequest)
        {
            var waiver = new CustomerWaiverRequest(
                customer.Id, customer.BranchId, request.OrderId, actorId, actorName, note, at);
            _db.CustomerWaiverRequests.Add(waiver);
            await _db.SaveChangesAsync(ct);
            waiverId = waiver.Id;
        }

        var decision = new CustomerDecision(
            customer.Id, customer.BranchId, actorId, actorName, type, note,
            null, null, null, snooze, waiverId, conversionId, at);
        _db.CustomerDecisions.Add(decision);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new PostBranchDecisionResultDto(
            decision.Id,
            type.ToString(),
            CustomerDecisionTypeLabels.Arabic(type),
            waiverId,
            conversionId);
    }

    private static bool TryResolveType(string raw, out CustomerDecisionType type)
    {
        if (Enum.TryParse(raw, true, out type)
            && type is CustomerDecisionType.Continue or CustomerDecisionType.FakeSale
                or CustomerDecisionType.LegalRequest or CustomerDecisionType.WaiverRequest)
            return true;
        return CustomerDecisionTypeLabels.TryParseArabic(raw, out type)
            && type is CustomerDecisionType.Continue or CustomerDecisionType.FakeSale
                or CustomerDecisionType.LegalRequest or CustomerDecisionType.WaiverRequest;
    }

    private async Task<bool> IsEligibleWeakPayerAsync(int customerId, bool isLegal, bool isFake, CancellationToken ct)
    {
        var today = _clock.Today;
        var weekStart = today.AddDays(-7);
        var now = _clock.UtcNowOffset;
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new { o.Id, o.SellAmount }).ToListAsync(ct);
        var oids = orders.Select(o => o.Id).ToList();
        var pays = oids.Count == 0
            ? []
            : await _db.InstallmentPayments.AsNoTracking()
                .Where(p => oids.Contains(p.OrderId))
                .Select(p => new { p.Amount, p.Date }).ToListAsync(ct);
        var sale = orders.Sum(o => o.SellAmount);
        var paid = pays.Sum(p => p.Amount);
        var week = pays.Where(p => p.Date >= weekStart).Sum(p => p.Amount);
        DateOnly? last = pays.Count == 0 ? null : pays.Max(p => p.Date);
        var snooze = await _db.CustomerDecisions.AsNoTracking().AnyAsync(d =>
            d.CustomerId == customerId && d.DecisionType == CustomerDecisionType.Continue
            && d.SnoozeUntil != null && d.SnoozeUntil > now, ct);
        var pendingLegal = await _db.LegalConversionRequests.AsNoTracking().AnyAsync(x =>
            x.CustomerId == customerId && x.Status == LegalConversionStatus.Pending, ct);
        var openWaiver = await _db.CustomerWaiverRequests.AsNoTracking().AnyAsync(x =>
            x.CustomerId == customerId
            && (x.Status == WaiverRequestStatus.Pending
                || x.Status == WaiverRequestStatus.AwaitingAccountant), ct);
        return WeakPayerRules.Matches(
            sale, week, sale - paid, last, today, isLegal, isFake, snooze, pendingLegal, openWaiver);
    }
}
