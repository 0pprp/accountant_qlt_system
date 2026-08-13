using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Customers.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerByIdHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetCustomerByIdResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .Select(x => new GetCustomerByIdResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                MotherName = x.MotherName,
                NationalCode = x.NationalCode,
                BirthDate = x.BirthDate,
                PhoneNumber = x.PhoneNumber,
                WhatsAppPhoneNumber = x.WhatsAppPhoneNumber,
                Business = x.Business,
                Branch = new GetBranchDto
                {
                    Id = x.BranchId,
                    Name = x.Branch!.Name
                },
                Attachments = x.Attachments.Select(a => new GetAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    RelativePath = a.RelativePath,
                    FileSizeInByte = a.FileSizeInByte,
                    Type = a.Type
                }).ToList()
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.CustomerNotFound;

        return customer;
    }
}