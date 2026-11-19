using Application.Archive.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Application.Archive.Queries.GetBundle;

public record GetArchiveBundleQuery : IRequest<Result<ArchiveBundleDetailDto>>
{
    public int BundleId { get; init; }
    public ArchiveStage? Stage { get; init; }
}
