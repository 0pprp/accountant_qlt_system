using Application.Common.Interfaces;
using Application.CustomerDecisions.Common;
using Application.SalesRequests.Common;
using Domain.Entities.CustomerDecisionAggregate;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.LegalConversionAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.CustomerDecisions.Commands;

file static class LegalConversionConcurrency
{
    public static async Task<Result> SaveAsync(Func<CancellationToken, Task> save, CancellationToken ct)
    {
        try
        {
            await save(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return CustomerDecisionErrors.ConversionConcurrency;
        }
    }
}

public sealed class ApproveLegalConversionCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
    public string? Note { get; init; }
}

public sealed class RejectLegalConversionCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
    public required string Reason { get; init; }
}

public sealed class ApproveLegalConversionHandler
    : IRequestHandler<ApproveLegalConversionCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ApproveLegalConversionHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(ApproveLegalConversionCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.LegalConversion.Decide) == false)
            return CustomerDecisionErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return CustomerDecisionErrors.Forbidden;

        var entity = await _db.LegalConversionRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return CustomerDecisionErrors.ConversionNotFound;

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == entity.CustomerId, ct);
        if (customer is null)
            return CustomerDecisionErrors.CustomerNotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            entity.Approve(actorId, actorName, request.Note, request.ExpectedVersion, at);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return CustomerDecisionErrors.ConversionConcurrency;
        }
        catch (InvalidOperationException)
        {
            return CustomerDecisionErrors.ConversionNotFound;
        }

        customer.MarkLegal();
        _db.CustomerDecisions.Add(new CustomerDecision(
            customer.Id, customer.BranchId, actorId, actorName,
            CustomerDecisionType.Legal,
            string.IsNullOrWhiteSpace(request.Note) ? "اعتماد تحويل قانونية" : request.Note.Trim(),
            null, null, null, null, null, entity.Id, at));
        var saved = await LegalConversionConcurrency.SaveAsync(ct2 => _db.SaveChangesAsync(ct2), ct);
        if (saved.IsFailed)
            return saved.Error!;
        await tx.CommitAsync(ct);
        return entity.Id;
    }
}

public sealed class RejectLegalConversionHandler
    : IRequestHandler<RejectLegalConversionCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public RejectLegalConversionHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RejectLegalConversionCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.LegalConversion.Decide) == false)
            return CustomerDecisionErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return CustomerDecisionErrors.Forbidden;
        if (string.IsNullOrWhiteSpace(request.Reason))
            return CustomerDecisionErrors.RejectReasonRequired;

        var entity = await _db.LegalConversionRequests.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return CustomerDecisionErrors.ConversionNotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        try
        {
            entity.Reject(actorId, actorName, request.Reason, request.ExpectedVersion, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return CustomerDecisionErrors.ConversionConcurrency;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("سبب"))
        {
            return CustomerDecisionErrors.RejectReasonRequired;
        }
        catch (InvalidOperationException)
        {
            return CustomerDecisionErrors.ConversionNotFound;
        }

        var saved = await LegalConversionConcurrency.SaveAsync(ct2 => _db.SaveChangesAsync(ct2), ct);
        if (saved.IsFailed)
            return saved.Error!;
        return entity.Id;
    }
}
