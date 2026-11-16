using Application.Common.Interfaces;
using Application.Committee.Common;
using Application.ManagementSalesMap.Common;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.SetCustomerLocation;

public record SetCommitteeCustomerLocationCommand(
    int UserId,
    int CustomerId,
    int ListId,
    double Latitude,
    double Longitude,
    int? OrderId = null) : IRequest<Result<CommitteeCustomerLocationDto>>;

public record CommitteeCustomerLocationDto
{
    public int CustomerId { get; set; }
    public bool HasLocation { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? OrderId { get; set; }
    public bool OrderLocationSaved { get; set; }
}

public class SetCommitteeCustomerLocationHandler
    : IRequestHandler<SetCommitteeCustomerLocationCommand, Result<CommitteeCustomerLocationDto>>
{
    private readonly IApplicationDbContext _db;

    public SetCommitteeCustomerLocationHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<CommitteeCustomerLocationDto>> Handle(
        SetCommitteeCustomerLocationCommand request,
        CancellationToken cancellationToken)
    {
        if (!await CommitteeAccess.OwnsListAsync(_db, request.UserId, request.ListId, cancellationToken))
            return CommitteeErrors.ListNotFound;

        var location = TryCreateLocation(request.Latitude, request.Longitude);
        if (location is null)
            return new Error("إحداثيات الموقع غير صالحة", "Committee_Location_Invalid");

        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer is null)
            return new Error("الزبون غير موجود", "Customer_NotFound");

        var onList = await _db.Orders.AsNoTracking()
            .AnyAsync(
                o => o.CustomerId == request.CustomerId && o.OrderListId == request.ListId,
                cancellationToken);
        if (!onList)
            return new Error("الزبون ليس ضمن هذه القائمة", "Committee_Customer_NotOnList");

        customer.Business.SetGps(location.Latitude, location.Longitude);

        int? savedOrderId = null;
        var orderLocationSaved = false;
        var order = await ResolveTargetOrderAsync(
            request.CustomerId,
            request.ListId,
            request.OrderId,
            cancellationToken);
        if (order is not null)
        {
            order.SetLocation(location);
            savedOrderId = order.Id;
            orderLocationSaved = true;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new CommitteeCustomerLocationDto
        {
            CustomerId = customer.Id,
            HasLocation = true,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            OrderId = savedOrderId,
            OrderLocationSaved = orderLocationSaved
        };
    }

    private async Task<Order?> ResolveTargetOrderAsync(
        int customerId,
        int listId,
        int? orderId,
        CancellationToken ct)
    {
        if (orderId is > 0)
        {
            return await _db.Orders
                .FirstOrDefaultAsync(
                    o => o.Id == orderId.Value
                         && o.CustomerId == customerId
                         && o.OrderListId == listId,
                    ct);
        }

        // تفضيل طلب قيد التنفيذ ثم طلب بانتظار المصادقة
        var inProgress = await _db.Orders
            .Where(o => o.CustomerId == customerId
                        && o.OrderListId == listId
                        && o.ExecutionStatus == OrderExecutionStatus.InProgress)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);
        if (inProgress is not null)
            return inProgress;

        return await _db.Orders
            .Where(o => o.CustomerId == customerId
                        && o.OrderListId == listId
                        && o.ExecutionStatus == OrderExecutionStatus.NotStarted)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static Location? TryCreateLocation(double lat, double lng)
    {
        try
        {
            var loc = new Location(lat, lng);
            return SalesMapGeo.TryGetValid(loc, out _, out _) ? loc : null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
