namespace Application.Users.Commands.Admin.UpdatePermissions;

public class UpdateUserPermissionsCommandValidator : AbstractValidator<UpdateUserPermissionsCommand>
{
    public UpdateUserPermissionsCommandValidator()
    {
        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithMessage("لا يمكن أن يكون الصلاحيات فارغًا.");
    }
}