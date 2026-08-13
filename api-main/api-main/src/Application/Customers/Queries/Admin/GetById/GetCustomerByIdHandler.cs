using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Common.Interfaces;
using Application.Customers.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.Admin.GetById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCustomerByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetCustomerByIdResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
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