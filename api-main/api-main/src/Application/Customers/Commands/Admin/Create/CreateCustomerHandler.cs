using Application.Common.Interfaces;
using Application.Customers.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Commands.Admin.Create;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand,
    Result<CreateCustomerResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public CreateCustomerHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateCustomerResponse>> Handle(CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var isNationalCodeExist = await _dbContext.Customers.AnyAsync(x => x.NationalCode == request.NationalCode, cancellationToken);
        if (isNationalCodeExist)
            return CustomerErrors.CustomerWithThisNationalCodeAlreadyExist;

        var customer = new Customer(
            request.FullName,
            request.MotherName,
            request.NationalCode,
            request.BirthDate,
            request.PhoneNumber,
            request.WhatsAppPhoneNumber,
            request.Business,
            request.BranchId);

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            customer.BranchId,
            ActivityType.CustomerCreated,
            TargetEntityType.Customer,
            customer.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCustomerResponse(customer.Id);
    }
}