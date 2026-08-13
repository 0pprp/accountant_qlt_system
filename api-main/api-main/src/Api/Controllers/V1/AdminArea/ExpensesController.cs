using Application.Expenses.Commands.Admin.Create;
using Application.Expenses.Commands.Admin.Update;
using Application.Expenses.Queries.Admin.GetById;
using Application.Expenses.Queries.Admin.GetExcelReport;
using Application.Expenses.Queries.Admin.GetLastFactorNumber;
using Application.Expenses.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ExpensesController : ApiController
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

	[HasPermission(Permissions.Expense.Create)]
	[HttpPost]
	public async Task<ActionResult> Create([FromForm] CreateExpenseCommand command)
	{
		var result = await _mediator.Send(command, CancellationToken);

		return result.ToHttpResponse();
	}

	[HasPermission(Permissions.Expense.Update)]
	[HttpPut("{id:int}")]
	public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateExpenseDto dto)
	{
		var command = new UpdateExpenseCommand
		{
			Id = id,
			ExpenseItems = dto.ExpenseItems
		};
		var result = await _mediator.Send(command, CancellationToken);

		return result.ToHttpResponse();
	}

	[HasPermission(Permissions.Expense.Read)]
	[HttpGet]
	public async Task<ActionResult<PaginatedList<GetPaginatedExpensesResponse>>> GetPaginated([FromQuery] Pagination pagination,
		[FromQuery] GetPaginatedExpensesFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
	{
		var query = new GetPaginatedExpensesQuery
		{
			Pagination = pagination,
			Filter = filter,
			SortCriteria = sortCriteria
		};
		var result = await _mediator.Send(query, CancellationToken);

		return result.ToHttpResponse();
	}

	[HasPermission(Permissions.Expense.Read)]
	[HttpGet("{id:int}")]
	public async Task<ActionResult<GetExpenseByIdResponse>> GetById([FromRoute] int id)
	{
		var query = new GetExpenseByIdQuery(id);
		var result = await _mediator.Send(query, CancellationToken);

		return result.ToHttpResponse();
	}

	[HasPermission(Permissions.Expense.Read)]
	[HttpGet("excel-report")]
	public async Task<ActionResult> GetExcelReport([FromQuery] GetExpensesExcelReportQuery query)
	{
		var result = await _mediator.Send(query, CancellationToken);

		var excelFile = result.Data;

		return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
	}
	
	[HasPermission(Permissions.Expense.Read)]
	[HttpGet("last-factor-number")]
	public async Task<ActionResult<GetLastExpenseFactorNumberResponse>> GetLastFactorNumber()
	{
		var query = new GetLastExpenseFactorNumberQuery();
		var result = await _mediator.Send(query, CancellationToken);

		return result.ToHttpResponse();
	}
}