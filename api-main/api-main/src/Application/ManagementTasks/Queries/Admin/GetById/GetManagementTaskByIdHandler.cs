using Application.Common.Interfaces;
using Application.ManagementTasks.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementTasks.Queries.Admin.GetById;

public class GetManagementTaskByIdHandler
    : IRequestHandler<GetManagementTaskByIdQuery, Result<ManagementTaskDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetManagementTaskByIdHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ManagementTaskDetailDto>> Handle(
        GetManagementTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var row = await _db.ManagementTasks
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new
            {
                Task = x,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                CreatedByName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null || ManagementTaskAccess.CanSeeTask(_currentUser, row.Task) == false)
            return ManagementTaskErrors.NotFound;

        var events = await _db.ManagementTaskEvents
            .AsNoTracking()
            .Where(x => x.TaskId == request.Id)
            .OrderBy(x => x.AtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var attachments = await _db.ManagementTaskAttachments
            .AsNoTracking()
            .Where(x => x.TaskId == request.Id)
            .OrderBy(x => x.UploadedAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return new ManagementTaskDetailDto(
            ManagementTaskMapper.ToListItem(row.Task, _clock.UtcNowOffset, row.BranchName, row.CreatedByName),
            events.Select(ManagementTaskMapper.ToEvent).ToList(),
            attachments.Select(ManagementTaskMapper.ToAttachment).ToList());
    }
}
