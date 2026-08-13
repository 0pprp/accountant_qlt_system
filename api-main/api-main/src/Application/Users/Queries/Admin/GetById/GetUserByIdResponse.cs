using System.Text.Json.Serialization;
using Application.Attachments.Common;
using Application.Branches.Common;
using Application.Permissions.Common;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;

namespace Application.Users.Queries.Admin.GetById;

public record GetUserByIdResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string MotherName { get; set; }
    public required string Username { get; set; }
    public required string NationalCode { get; set; }
    public DateOnly BirthDate { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public UserCreationStep CreationStep { get; set; }
    public SalaryDetail? SalaryDetail { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
    public required List<GetBranchDto> Branches { get; set; }
    public required List<GetRoleDto> Roles { get; set; }
    [JsonIgnore]
    public List<GetPermissionDto> Permissions { get; set; }
    [JsonIgnore]
    public List<GetPermissionDto> CustomPermissions { get; set; }
    public List<GetPermissionDto> AllPermissions => Permissions.Union(CustomPermissions).ToList();
}

public record GetRoleDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}