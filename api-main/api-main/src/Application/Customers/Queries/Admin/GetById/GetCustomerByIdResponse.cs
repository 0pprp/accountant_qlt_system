using Application.Attachments.Common;
using Application.Branches.Common;
using Domain.Entities.CustomerAggregate;

namespace Application.Customers.Queries.Admin.GetById;

public record GetCustomerByIdResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public required string WhatsAppPhoneNumber { get; set; }
    public required Business Business { get; set; }
    public required GetBranchDto Branch { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
}