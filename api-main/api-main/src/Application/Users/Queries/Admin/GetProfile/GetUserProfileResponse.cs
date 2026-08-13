using Application.Attachments.Common;

namespace Application.Users.Queries.Admin.GetProfile;

public record GetUserProfileResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public GetAttachmentDto? ProfilePicture { get; set; }
    public required List<string> Roles { get; set; }
}