using Domain.Entities.CustomerAggregate;

namespace Application.Customers.Commands.Admin.Create;

public record CreateCustomerCommand : IRequest<Result<CreateCustomerResponse>>
{
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public required string WhatsAppPhoneNumber { get; set; }
    public required Business Business { get; set; }
    public int BranchId { get; set; }
}