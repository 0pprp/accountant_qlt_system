using Application.Common.Interfaces;
using Application.Customers.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Commands.Admin.Update;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateCustomerHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.CustomerNotFound;

        customer.Update(
            request.FullName,
            request.MotherName,
            request.NationalCode,
            request.BirthDate,
            request.PhoneNumber,
            request.WhatsAppPhoneNumber,
            request.Business,
            request.BranchId);

        await _activityLogService.AddAsync(
            customer.BranchId,
            ActivityType.CustomerUpdated,
            TargetEntityType.Customer,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}