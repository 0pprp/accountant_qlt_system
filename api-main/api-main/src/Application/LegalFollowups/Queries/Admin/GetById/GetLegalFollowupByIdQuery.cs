using Application.LegalFollowups.Common;

namespace Application.LegalFollowups.Queries.Admin.GetById;

public sealed class GetLegalFollowupByIdQuery : IRequest<Result<LegalFollowupCaseRowDto>>
{
    public int CaseId { get; init; }
}
