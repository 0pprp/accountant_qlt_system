using Application.Common.Interfaces;
using Application.PriceBoard.Common;
using Application.SalesRequests.Common;
using Domain.Entities.PriceBoardAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.PriceBoard.Commands;

public sealed class CreatePriceBoardEntryCommand : IRequest<Result<PriceBoardEntryDto>>
{
    public required string ProductName { get; init; }
    public decimal SalePrice { get; init; }
    public decimal DailyInstallment { get; init; }
    public decimal DownPayment { get; init; }
    public int TargetProvinceId { get; init; }
}

public sealed class UpdatePriceBoardEntryCommand : IRequest<Result<PriceBoardEntryDto>>
{
    public int Id { get; init; }
    public required string ProductName { get; init; }
    public decimal SalePrice { get; init; }
    public decimal DailyInstallment { get; init; }
    public decimal DownPayment { get; init; }
    public int TargetProvinceId { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class DeletePriceBoardEntryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

public sealed class CreatePriceBoardEntryHandler
    : IRequestHandler<CreatePriceBoardEntryCommand, Result<PriceBoardEntryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public CreatePriceBoardEntryHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<PriceBoardEntryDto>> Handle(CreatePriceBoardEntryCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.PriceBoard.Create) == false)
            return PriceBoardErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return PriceBoardErrors.Forbidden;

        var province = await _db.Provinces.AsNoTracking()
            .Where(p => p.Id == request.TargetProvinceId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync(ct);
        if (province is null)
            return PriceBoardErrors.ProvinceNotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);

        PriceBoardEntry entity;
        try
        {
            entity = new PriceBoardEntry(
                request.ProductName, request.SalePrice, request.DailyInstallment, request.DownPayment,
                province.Id, province.Name, actorId, actorName, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException)
        {
            return PriceBoardErrors.Validation;
        }

        _db.PriceBoardEntries.Add(entity);
        await _db.SaveChangesAsync(ct);
        return PriceBoardMapper.ToDto(entity);
    }
}

public sealed class UpdatePriceBoardEntryHandler
    : IRequestHandler<UpdatePriceBoardEntryCommand, Result<PriceBoardEntryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public UpdatePriceBoardEntryHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<PriceBoardEntryDto>> Handle(UpdatePriceBoardEntryCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.PriceBoard.Update) == false)
            return PriceBoardErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return PriceBoardErrors.Forbidden;

        var entity = await _db.PriceBoardEntries.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return PriceBoardErrors.NotFound;

        var province = await _db.Provinces.AsNoTracking()
            .Where(p => p.Id == request.TargetProvinceId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync(ct);
        if (province is null)
            return PriceBoardErrors.ProvinceNotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);

        try
        {
            entity.Update(
                request.ProductName, request.SalePrice, request.DailyInstallment, request.DownPayment,
                province.Id, province.Name, actorId, actorName, request.ExpectedVersion, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return PriceBoardErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return PriceBoardErrors.Validation;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return PriceBoardErrors.Concurrency;
        }

        return PriceBoardMapper.ToDto(entity);
    }
}

public sealed class DeletePriceBoardEntryHandler : IRequestHandler<DeletePriceBoardEntryCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public DeletePriceBoardEntryHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result> Handle(DeletePriceBoardEntryCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.PriceBoard.Delete) == false)
            return PriceBoardErrors.Forbidden;

        var entity = await _db.PriceBoardEntries.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return PriceBoardErrors.NotFound;
        if (entity.MatchesExpectedVersion(request.ExpectedVersion) == false)
            return PriceBoardErrors.Concurrency;

        _db.PriceBoardEntries.Remove(entity);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return PriceBoardErrors.Concurrency;
        }

        return Result.Success();
    }
}
