using Application.Attachments.Common;
using Application.Branches.Common;

namespace Application.Users.Queries.App.GetProfile;

public record GetUserProfileResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required List<GetBranchDto> Branches { get; set; }
    public GetAttachmentDto? ProfilePicture { get; set; }
    public required string PhoneNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Address { get; set; }
    public OrderListDto? OrderList { get; set; }
}

public record OrderListDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}