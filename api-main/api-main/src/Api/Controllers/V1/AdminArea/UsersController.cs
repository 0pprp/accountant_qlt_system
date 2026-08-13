using Application.Users.Commands.Admin.CompletePersonalDocuments;
using Application.Users.Commands.Admin.Create;
using Application.Users.Commands.Admin.Update;
using Application.Users.Commands.Admin.UpdatePermissions;
using Application.Users.Commands.Admin.UpdateSalaryDetail;
using Application.Users.Queries.Admin.GetById;
using Application.Users.Queries.Admin.GetAvailableColumns;
using Application.Users.Queries.Admin.GetPaginated;
using Application.Users.Queries.Admin.GetProfile;
using Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.User.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateUserResponse>> Create([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Update)]
    [HttpPut("{id:int}/personal-documents/complete")]
    public async Task<ActionResult> CompletePersonalDocuments([FromRoute] int id)
    {
        var command = new CompleteUserPersonalDocumentsCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Update)]
    [HttpPut("{id:int}/salary-detail")]
    public async Task<ActionResult> UpdateSalaryDetail([FromRoute] int id, [FromBody] UpdateUserSalaryDetailDto dto)
    {
        var command = new UpdateUserSalaryDetailCommand
        {
            UserId = id,
            Type = dto.Type,
            Amount = dto.Amount,
            SaleSharePercent = dto.SaleSharePercent,
            InstallmentSharePercent = dto.InstallmentSharePercent
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Update)]
    [HttpPut("{id:int}/permissions")]
    public async Task<ActionResult> UpdatePermissions([FromRoute] int id, [FromBody] UpdateUserPermissionsDto dto)
    {
        var command = new UpdateUserPermissionsCommand
        {
            UserId = id,
            PermissionIds = dto.PermissionIds
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetUserByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet]
    public async Task<ActionResult<GetPaginatedUsersResponse>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedUsersFilterDto filter, [FromQuery] List<string>? columns,
        [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedUsersQuery
        {
            Pagination = pagination,
            Filter = filter,
            SelectedColumns = columns,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet("available-columns")]
    public async Task<ActionResult<List<GetAvailableUserColumnsResponse>>> GetAvailableColumns()
    {
        var query = new GetAvailableUserColumnsQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateUserDto dto)
    {
        var command = new UpdateUserCommand
        {
            Id = id,
            FullName = dto.FullName,
            MotherName = dto.MotherName,
            Username = dto.Username,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            RoleId = dto.RoleId,
            BranchIds = dto.BranchIds,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet("{id:int}/daily-installment-report")]
    public async Task<ActionResult<GetUserDailyInstallmentCollectionsReportResponse>> GetDailyInstallmentReport([FromRoute] int id,
        [FromQuery] GetUserDailyInstallmentCollectionsReportFilter filter)
    {
        var query = new GetUserDailyInstallmentCollectionsReportQuery
        {
            UserId = id,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.User.Read)]
    [HttpGet("profile")]
    public async Task<ActionResult<GetUserProfileResponse>> GetProfile()
    {
        var query = new GetUserProfileQuery(_currentUserService.UserId!.Value);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}