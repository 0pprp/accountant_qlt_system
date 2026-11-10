using Domain.Common;

namespace Domain.Entities.LegalFollowupAggregate;

/// <summary>مستمسك قضية قانونية — ليس جدول قضايا موازيًا.</summary>
public class LegalFollowupCaseDocument : Entity
{
    private LegalFollowupCaseDocument()
    {
    }

    public LegalFollowupCaseDocument(
        int caseId,
        string documentType,
        string fileName,
        string storedFileName,
        string relativePath,
        string contentType,
        string extension,
        long sizeBytes,
        int uploadedByUserId,
        DateTimeOffset uploadedAtUtc)
    {
        CaseId = caseId;
        DocumentType = documentType;
        FileName = fileName;
        StoredFileName = storedFileName;
        RelativePath = relativePath;
        ContentType = contentType;
        Extension = extension;
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    public int CaseId { get; private set; }
    public string DocumentType { get; private set; } = "";
    public string FileName { get; private set; } = "";
    public string StoredFileName { get; private set; } = "";
    public string RelativePath { get; private set; } = "";
    public string ContentType { get; private set; } = "";
    public string Extension { get; private set; } = "";
    public long SizeBytes { get; private set; }
    public int UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

    public LegalFollowupCase? Case { get; private set; }
}
