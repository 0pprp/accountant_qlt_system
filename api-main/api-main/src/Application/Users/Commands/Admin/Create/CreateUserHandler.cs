using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.Admin.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CreateUserHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var fullName = NormalizePersonName(request.FullName);
        var username = request.Username.Trim();
        // كلمة السر كما أُرسلت — لا تُقصّ المسافات بصمت.
        var password = request.Password;
        var motherName = NormalizeOptional(request.MotherName);
        var nationalCode = NormalizeOptional(request.NationalCode);
        var address = NormalizeOptional(request.Address);
        var phoneNumber = NormalizeOptional(request.PhoneNumber);
        var branchIds = request.BranchIds.Distinct().ToList();

        var assignment = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(
            _dbContext,
            _currentUserService,
            targetUserId: null,
            requestedRoleId: request.RoleId,
            requestedBranchIds: branchIds,
            currentRoleId: null,
            currentBranchIds: [],
            cancellationToken);
        if (assignment.IsFailed)
            return assignment.Error!.Value;

        var isUsernameExist = await _dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken);
        if (isUsernameExist)
            return UserErrors.UsernameIsAlreadyExist;

        var hashedPassword = PasswordHash.HashPassword(password);
        var user = new User(
            fullName,
            motherName,
            nationalCode,
            request.BirthDate,
            username,
            hashedPassword,
            address,
            phoneNumber)
        {
            CreationStep = UserCreationStep.Completed,
            UserBranches = branchIds.Select(x => new UserBranch(x)).ToHashSet(),
            UserRoles = [new UserRole(request.RoleId)]
        };
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var branchId in branchIds)
            {
                await _activityLogService.AddAsync(
                    branchId,
                    ActivityType.UserCreated,
                    TargetEntityType.User,
                    user.Id,
                    details: $"roleId={request.RoleId}; branches=[{string.Join(',', branchIds)}]");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueUsernameViolation(ex))
        {
            return UserErrors.UsernameIsAlreadyExist;
        }

        return new CreateUserResponse(user.Id);
    }

    /// <summary>تنظيف الفراغات الزائدة مع الحفاظ على الأسماء المركّبة ككلمات.</summary>
    public static string NormalizePersonName(string raw)
    {
        var parts = raw.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static string? NormalizeOptional(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var parts = raw.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static bool IsUniqueUsernameViolation(DbUpdateException ex)
    {
        var message = $"{ex.InnerException?.Message} {ex.Message}";
        return message.Contains("Username", StringComparison.OrdinalIgnoreCase)
            && (message.Contains("23505", StringComparison.Ordinal)
                || message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }
}
