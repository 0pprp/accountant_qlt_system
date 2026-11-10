using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Queries.Admin.GetById;

public record GetManagementTaskByIdQuery(int Id) : IRequest<Result<ManagementTaskDetailDto>>;
