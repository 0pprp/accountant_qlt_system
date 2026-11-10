using Domain.Common;

namespace Domain.Entities.ManagementTaskAggregate;

public class ManagementTaskAttachment : Entity
{
    private ManagementTaskAttachment()
    {
    }

    public ManagementTaskAttachment(
        int taskId,
        string originalFileName,
        string storedFileName,
        string relativePath,
        string contentType,
        string fileExtension,
        long sizeBytes,
        int uploadedByUserId,
        DateTimeOffset uploadedAtUtc)
    {
        TaskId = taskId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        RelativePath = relativePath;
        ContentType = contentType;
        FileExtension = fileExtension;
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    public int TaskId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string RelativePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public int UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

    public ManagementTask? Task { get; private set; }
}
