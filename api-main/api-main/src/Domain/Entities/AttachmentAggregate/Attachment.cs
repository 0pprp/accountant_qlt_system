using Domain.Common;
using Domain.Entities.AttachmentAggregate.Enums;

namespace Domain.Entities.AttachmentAggregate;

public class Attachment : AuditableEntity
{
    public Attachment(
        string originalFileName,
        string fileName,
        string relativePath,
        string contentType,
        string fileExtension,
        long fileSizeInByte,
        AttachmentType type,
        FileType fileType)
    {
        OriginalFileName = originalFileName;
        FileName = fileName;
        RelativePath = relativePath;
        ContentType = contentType;
        FileExtension = fileExtension;
        FileSizeInByte = fileSizeInByte;
        Type = type;
        FileType = fileType;
    }

    public string OriginalFileName { get; set; }
    public string FileName { get; set; }
    public string RelativePath { get; set; }
    public string ContentType { get; set; }
    public string FileExtension { get; set; }
    public long FileSizeInByte { get; set; }
    public AttachmentType Type { get; set; }
    public FileType FileType { get; set; }
    public int? UserId { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int? PurchaseId { get; set; }
    public int? ExpenseId { get; set; }

    public void Update(string originalFileName, string fileName, string relativePath, string contentType, string fileExtension,
        long fileSizeInByte, FileType fileType)
    {
        OriginalFileName = originalFileName;
        FileName = fileName;
        RelativePath = relativePath;
        ContentType = contentType;
        FileExtension = fileExtension;
        FileSizeInByte = fileSizeInByte;
        FileType = fileType;
    }
}