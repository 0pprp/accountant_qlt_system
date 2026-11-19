namespace Application.Safes.Queries.Admin.GetAll;

/// <summary>خزائن فرع واحد. يُقيَّد بنطاق فروع الجلسة.</summary>
public record GetAllSafesQuery(int BranchId) : IRequest<Result<List<GetAllSafesResponse>>>;
