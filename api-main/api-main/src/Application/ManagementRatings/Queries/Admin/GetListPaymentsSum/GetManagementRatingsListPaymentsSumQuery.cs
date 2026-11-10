using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetListPaymentsSum;

public record GetManagementRatingsListPaymentsSumQuery : IRequest<Result<ManagementRatingsListPaymentsSumResponse>>
{
    public int BranchId { get; init; }
    public int ListId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}
