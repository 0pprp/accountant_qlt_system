using Application.FollowerWork.Common;

namespace Application.SalesLocations.Commands.App.IngestBatch;

public class IngestSalesLocationBatchCommandValidator : AbstractValidator<IngestSalesLocationBatchCommand>
{
    public IngestSalesLocationBatchCommandValidator()
    {
        RuleFor(x => x.Points).NotEmpty();
        RuleFor(x => x.Points.Count).LessThanOrEqualTo(FollowerLocationRules.MaxBatchPoints);
    }
}
