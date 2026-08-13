using Domain.Entities.CustomerAggregate;

namespace Application.Customers.Commands.App.Update;

public record UpdateCustomerCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public required string WhatsAppPhoneNumber { get; set; }
    public required Business Business { get; set; }
}

public record UpdateCustomerDto
{
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public required string WhatsAppPhoneNumber { get; set; }
    public required Business Business { get; set; }
}