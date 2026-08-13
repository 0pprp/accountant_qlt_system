namespace Application.Expenses.Queries.Admin.GetById;

public record GetExpenseByIdQuery(int Id) : IRequest<Result<GetExpenseByIdResponse>>;
