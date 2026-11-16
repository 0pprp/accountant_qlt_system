using Application.Users.Commands.Admin.Create;
using FluentValidation.TestHelper;

namespace Application.Tests.Users;

public class CreateUserCommandTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void NormalizePersonName_CollapsesExtraSpaces_KeepsCompoundTokens()
    {
        Assert.Equal(
            "عبد الله أحمد",
            CreateUserHandler.NormalizePersonName("  عبد   الله    أحمد  "));
    }

    [Fact]
    public void Validator_AcceptsMinimalFiveFieldPayload()
    {
        var cmd = new CreateUserCommand
        {
            Username = "seller01",
            Password = "secret12",
            FullName = "علي حسن كريم",
            RoleId = 7,
            BranchIds = [3],
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_RejectsShortPassword_WithoutTrimmingSpacesAway()
    {
        var cmd = new CreateUserCommand
        {
            Username = "seller01",
            Password = "1234567",
            FullName = "علي حسن كريم",
            RoleId = 7,
            BranchIds = [3],
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validator_AllowsPasswordWithInternalSpaces_OfValidLength()
    {
        var cmd = new CreateUserCommand
        {
            Username = "seller01",
            Password = "pass word",
            FullName = "علي حسن كريم",
            RoleId = 7,
            BranchIds = [3],
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validator_DoesNotRequirePersonalProfileFields()
    {
        var cmd = new CreateUserCommand
        {
            Username = "seller01",
            Password = "secret12",
            FullName = "علي حسن كريم",
            RoleId = 7,
            BranchIds = [3],
            MotherName = null,
            NationalCode = null,
            BirthDate = null,
            Address = null,
            PhoneNumber = null,
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.MotherName);
        result.ShouldNotHaveValidationErrorFor(x => x.NationalCode);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validator_RequiresRoleAndBranch()
    {
        var cmd = new CreateUserCommand
        {
            Username = "seller01",
            Password = "secret12",
            FullName = "علي حسن كريم",
            RoleId = 0,
            BranchIds = [],
        };

        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
        result.ShouldHaveValidationErrorFor(x => x.BranchIds);
    }
}
