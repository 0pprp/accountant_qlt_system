using Application.Customers.Commands.Admin.Create;
using Application.Customers.Commands.Admin.Update;
using Application.Customers.Queries.Admin.GetById;
using Application.Customers.Queries.Admin.GetExcelReport;
using Application.Customers.Queries.Admin.GetOrders;
using Application.Customers.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class CustomersController : ApiController
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Customer.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateCustomerResponse>> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedCustomersResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedCustomersFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedCustomersQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetCustomerByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateCustomerDto dto)
    {
        var command = new UpdateCustomerCommand
        {
            Id = id,
            FullName = dto.FullName,
            MotherName = dto.MotherName,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            PhoneNumber = dto.PhoneNumber,
            WhatsAppPhoneNumber = dto.WhatsAppPhoneNumber,
            Business = dto.Business,
            BranchId = dto.BranchId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult> GetExcelReport([FromQuery] GetPaginatedCustomersFilter filter)
    {
        var query = new GetCustomersExcelReportQuery(filter);
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<List<GetCustomerOrdersResponse>>> GetOrders([FromRoute] int id)
    {
        var query = new GetCustomerOrdersQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}