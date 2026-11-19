using Application.Archive.Common;

namespace Application.Archive.Queries.GetFlowBoard;

/// <summary>لوحة التدفّق لمسؤول الأرشيف — أين كل حزمة، وكم استُلم منها، وبأي سرعة تمشي.</summary>
public record GetArchiveFlowBoardQuery : IRequest<Result<List<ArchiveFlowRowDto>>>
{
    public int? BranchId { get; init; }
    public int Limit { get; init; } = 50;
}
