using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskMapper
{
    private static readonly TimeZoneInfo IraqTz = ResolveIraq();

    public static DateOnly IraqToday(DateTimeOffset utcNow)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow.UtcDateTime, IraqTz);
        return DateOnly.FromDateTime(local);
    }

    public static ManagementTaskListItemDto ToListItem(
        ManagementTask task,
        DateTimeOffset utcNow,
        string? branchName = null,
        string? createdByDisplayName = null)
    {
        return new ManagementTaskListItemDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.DueAtUtc,
            task.IsOverdue(utcNow, IraqToday(utcNow)),
            task.BranchId,
            branchName,
            task.CreatedByUserId,
            createdByDisplayName,
            task.AssigneeUserId,
            task.AssigneeDisplayName,
            task.CurrentHolderUserId,
            task.CurrentHolderDisplayName,
            task.RejectNote,
            task.CustomerId,
            task.OrderId,
            task.ConcurrencyStamp,
            task.CreatedAt,
            task.StatusChangedAtUtc,
            task.TransferredAtUtc,
            task.BranchCompletedAtUtc,
            task.AssigneeCompletedAtUtc,
            task.ConfirmedAtUtc);
    }

    public static ManagementTaskAttachmentDto ToAttachment(ManagementTaskAttachment a) =>
        new(a.Id, a.OriginalFileName, a.ContentType, a.SizeBytes, a.UploadedAtUtc, a.UploadedByUserId);

    public static ManagementTaskEventDto ToEvent(ManagementTaskEvent e) =>
        new(
            e.Id,
            e.EventType,
            e.AtUtc,
            e.ActorUserId,
            e.ActorDisplayName,
            e.TargetUserId,
            e.TargetDisplayName,
            e.Note,
            e.PreviousStatus,
            e.NewStatus);

    public static ManagementTaskActionResultDto ToActionResult(ManagementTask task) =>
        new(task.Id, task.Status, task.ConcurrencyStamp);

    public static bool IsAllowedExtension(string extension)
    {
        var ext = extension.Trim().ToLowerInvariant();
        if (ext.StartsWith('.') == false)
            ext = "." + ext;
        return AllowedExtensions.Contains(ext);
    }

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx"
    };

    public const long MaxAttachmentBytes = 20L * 1024 * 1024;

    private static TimeZoneInfo ResolveIraq()
    {
        foreach (var id in new[] { "Asia/Baghdad", "Arabic Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("Iraq(+03)", TimeSpan.FromHours(3), "Iraq", "Iraq");
    }
}
