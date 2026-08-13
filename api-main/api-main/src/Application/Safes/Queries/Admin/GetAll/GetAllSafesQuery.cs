namespace Application.Safes.Queries.Admin.GetAll;

public record GetAllSafesQuery : IRequest<Result<List<GetAllSafesResponse>>>;