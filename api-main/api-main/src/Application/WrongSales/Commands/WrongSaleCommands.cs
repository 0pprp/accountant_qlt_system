using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.WrongSales.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.WrongSales.Commands;

public sealed class MarkWrongSaleDelegateReadCommand : IRequest<Result<WrongSaleNoticeDto>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class MarkWrongSaleBranchReviewedCommand : IRequest<Result<WrongSaleNoticeDto>>
{
    public int Id { get; init; }
    public string? Note { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class MarkWrongSaleDelegateReadHandler
    : IRequestHandler<MarkWrongSaleDelegateReadCommand, Result<WrongSaleNoticeDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkWrongSaleDelegateReadHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WrongSaleNoticeDto>> Handle(
        MarkWrongSaleDelegateReadCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.WrongSale.MarkDelegateRead) == false)
            return WrongSaleErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WrongSaleErrors.Forbidden;

        var entity = await _db.WrongSaleNotices.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return WrongSaleErrors.NotFound;
        if (InBranchScope(entity.BranchId) == false)
            return WrongSaleErrors.BranchScope;

        try
        {
            // إعادة التعليم = idempotent نجاح بلا أثر إضافي
            entity.MarkDelegateRead(actorId, _clock.UtcNowOffset, request.ExpectedVersion);
        }
        catch (InvalidOperationException)
        {
            return WrongSaleErrors.Concurrency;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return WrongSaleErrors.Concurrency;
        }

        return WrongSaleMapper.ToDto(entity);
    }

    private bool InBranchScope(int branchId)
    {
        var scope = _current.BranchIds;
        if (scope is null || scope.Count == 0)
            return false;
        return scope.Contains(branchId);
    }
}

public sealed class MarkWrongSaleBranchReviewedHandler
    : IRequestHandler<MarkWrongSaleBranchReviewedCommand, Result<WrongSaleNoticeDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public MarkWrongSaleBranchReviewedHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WrongSaleNoticeDto>> Handle(
        MarkWrongSaleBranchReviewedCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.WrongSale.MarkBranchReviewed) == false)
            return WrongSaleErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WrongSaleErrors.Forbidden;

        var entity = await _db.WrongSaleNotices.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return WrongSaleErrors.NotFound;

        var scope = _current.BranchIds;
        if (scope is null || scope.Count == 0 || scope.Contains(entity.BranchId) == false)
            return WrongSaleErrors.BranchScope;

        try
        {
            entity.MarkBranchReviewed(actorId, request.Note, _clock.UtcNowOffset, request.ExpectedVersion);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("مسبقاً"))
        {
            return WrongSaleErrors.AlreadyReviewed;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ملاحظة"))
        {
            return WrongSaleErrors.NoteRequired;
        }
        catch (InvalidOperationException)
        {
            return WrongSaleErrors.Concurrency;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return WrongSaleErrors.Concurrency;
        }

        return WrongSaleMapper.ToDto(entity);
    }
}
