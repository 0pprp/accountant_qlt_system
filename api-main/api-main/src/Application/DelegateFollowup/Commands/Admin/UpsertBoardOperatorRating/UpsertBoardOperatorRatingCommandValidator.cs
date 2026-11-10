using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Commands.Admin.UpsertBoardOperatorRating;

public class UpsertBoardOperatorRatingCommandValidator : AbstractValidator<UpsertBoardOperatorRatingCommand>
{
    public UpsertBoardOperatorRatingCommandValidator()
    {
        RuleFor(x => x.ListId)
            .GreaterThan(0)
            .WithMessage("معرّف القائمة غير صالح");

        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .WithMessage("معرف الفرع مطلوب");

        RuleFor(x => x.Score)
            .InclusiveBetween(
                DelegateFollowupRules.BoardOperatorScoreMin,
                DelegateFollowupRules.BoardOperatorScoreMax)
            .WithMessage("التقييم يجب أن يكون بين 0 و 10");

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage("يجب ألا تتجاوز الملاحظة 500 حرف");
    }
}
