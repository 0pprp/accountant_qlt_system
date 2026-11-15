using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Queries.Admin.AudiencePreview;

public record GetManagementTaskAudiencePreviewQuery : IRequest<Result<ManagementTaskAudiencePreviewDto>>
{
    public int BranchId { get; init; }
    public ManagementTaskAudience Audience { get; init; }
}
