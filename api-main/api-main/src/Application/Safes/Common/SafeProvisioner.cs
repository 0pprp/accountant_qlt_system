using Application.Common.Interfaces;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SafeEntity = Domain.Entities.SafeAggregate.Safe;

namespace Application.Safes.Common;

/// <summary>
/// تهيئة الخزائن تلقائياً وإعادة تسميتها عند تغيير اسم مصدرها. كل الطرق idempotent
/// وتحفظ بنفسها، فلا يصل تسديد إلى قائمة بلا خزينة.
/// </summary>
public class SafeProvisioner
{
    private readonly IApplicationDbContext _dbContext;

    public SafeProvisioner(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>قاصة الفرع وسجل الصرفيات، مع مزامنة الاسم عند تغيير اسم الفرع.</summary>
    public async Task<int> EnsureBranchSafesAsync(int branchId, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Branches.AsNoTracking()
            .Where(x => x.Id == branchId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null)
            return 0;

        var existing = await _dbContext.Safes
            .Where(x => x.BranchId == branchId && (x.Kind == SafeKind.Branch || x.Kind == SafeKind.Expenses))
            .ToListAsync(cancellationToken);

        var branchSafe = existing.FirstOrDefault(x => x.Kind == SafeKind.Branch);
        if (branchSafe is null)
            _dbContext.Safes.Add(SafeEntity.ForBranch(branch.Name, branchId));
        else
            branchSafe.Rename(SafeNames.ForBranch(branch.Name));

        var expensesSafe = existing.FirstOrDefault(x => x.Kind == SafeKind.Expenses);
        if (expensesSafe is null)
            _dbContext.Safes.Add(SafeEntity.ForExpenses(branch.Name, branchId));
        else
            expensesSafe.Rename(SafeNames.ForExpenses(branch.Name));

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>خزينة القائمة، مع مزامنة الاسم وتصحيح الفرع عند نقل القائمة.</summary>
    public async Task<int> EnsureOrderListSafeAsync(int orderListId, CancellationToken cancellationToken)
    {
        var list = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.Id == orderListId)
            .Select(x => new { x.Id, x.Name, x.BranchId, x.MotabaId })
            .FirstOrDefaultAsync(cancellationToken);

        if (list is null)
            return 0;

        var safe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.OrderListId == orderListId, cancellationToken);

        if (safe is null)
            _dbContext.Safes.Add(SafeEntity.ForOrderList(list.Name, list.BranchId, orderListId));
        else
            safe.Rename(SafeNames.ForOrderList(list.Name));

        var saved = await _dbContext.SaveChangesAsync(cancellationToken);
        saved += await EnsureEmployeeSafeAsync(list.MotabaId, list.BranchId, cancellationToken);
        return saved;
    }

    /// <summary>خزينة موظف — المتابع المُحصِّل أو موظف المبيعات.</summary>
    public async Task<int> EnsureEmployeeSafeAsync(int userId, int branchId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new { x.Id, x.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return 0;

        var safe = await _dbContext.Safes
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.UserId == userId, cancellationToken);

        if (safe is null)
            _dbContext.Safes.Add(SafeEntity.ForEmployee(user.FullName, branchId, userId));
        else
            safe.Rename(SafeNames.ForEmployee(user.FullName));

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// ترميم شامل: خزائن كل الفروع، وكل القوائم، وكل متابع مكلّف، وكل موظف مبيعات بتكليف نشط.
    /// يُنادى عند الإقلاع وبعد هجرة الترحيل.
    /// </summary>
    public async Task<int> RepairAllAsync(CancellationToken cancellationToken)
    {
        var changes = 0;

        var branchIds = await _dbContext.Branches.AsNoTracking()
            .Select(x => x.Id).ToListAsync(cancellationToken);

        foreach (var branchId in branchIds)
            changes += await EnsureBranchSafesAsync(branchId, cancellationToken);

        var listIds = await _dbContext.OrderLists.AsNoTracking()
            .Select(x => x.Id).ToListAsync(cancellationToken);

        foreach (var listId in listIds)
            changes += await EnsureOrderListSafeAsync(listId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var salesEmployees = await _dbContext.SalesEmployeeAssignments.AsNoTracking()
            .Where(a => a.StartsAtUtc <= now && (a.EndsAtUtc == null || a.EndsAtUtc > now))
            .Select(a => new { a.UserId, a.BranchId })
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var assignment in salesEmployees)
            changes += await EnsureEmployeeSafeAsync(assignment.UserId, assignment.BranchId, cancellationToken);

        return changes;
    }
}
