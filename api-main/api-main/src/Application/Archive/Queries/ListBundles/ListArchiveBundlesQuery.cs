using Application.Archive.Common;
using Application.Common.Models;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Queries.ListBundles;

/// <summary>
/// كل طرف يرى شريحته: موظف المبيعات حزمه هو، والمراحل الوسطى ما يخصّها،
/// ومن يملك الإشراف يرى كل حزم فروعه.
/// </summary>
public record ListArchiveBundlesQuery : IRequest<Result<PaginatedList<ArchiveBundleListItemDto>>>
{
    public required Pagination Pagination { get; init; }
    public int? BranchId { get; init; }
    public ArchiveStage? Stage { get; init; }
    public int? Year { get; init; }
    public int? Month { get; init; }

    /// <summary>الحزم التي تنتظر فعلاً من السائل — صندوق الوارد.</summary>
    public bool PendingOnly { get; init; }
}
