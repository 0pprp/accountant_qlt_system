namespace Application.FollowerWork.Commands.Admin.DeleteFollower;

public class DeleteFollowerCommandValidator : AbstractValidator<DeleteFollowerCommand>
{
    public DeleteFollowerCommandValidator()
    {
        RuleFor(x => x.FollowerId)
            .GreaterThan(0)
            .WithMessage("معرّف المتابع غير صالح");
    }
}
