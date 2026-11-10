using Application.FollowerWork.Common;
using FluentValidation;

namespace Application.FollowerWork.Queries.Admin.GetFollowers;

public sealed class GetFollowerWorkFollowersQueryValidator : AbstractValidator<GetFollowerWorkFollowersQuery>
{
    public GetFollowerWorkFollowersQueryValidator()
    {
        RuleFor(x => x.BranchId)
            .Must(id => id is null or > 0)
            .WithMessage("معرّف الفرع غير صالح.");

        RuleFor(x => x.Page)
            .Must(p => p is null or >= 1)
            .WithMessage("رقم الصفحة غير صالح.");

        RuleFor(x => x.PageSize)
            .Must(p => p is null or (>= 1 and <= FollowerWorkRules.MaxPageSize))
            .WithMessage($"حجم الصفحة بين 1 و{FollowerWorkRules.MaxPageSize}.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => x.Search is not null);

        RuleFor(x => x.Sort)
            .Must(s => string.IsNullOrWhiteSpace(s) || FollowerWorkRules.AllowedSorts.Contains(s.Trim()))
            .WithMessage("ترتيب غير مسموح.");
    }
}
