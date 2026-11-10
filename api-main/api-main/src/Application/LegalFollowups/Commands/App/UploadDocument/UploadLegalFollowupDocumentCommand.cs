using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.LegalFollowups.Common;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Commands.App.UploadDocument;

public sealed class UploadLegalFollowupDocumentCommand : IRequest<Result<LegalFollowupDocumentDto>>
{
    public int CaseId { get; init; }
    public string DocumentType { get; init; } = "";
    public IFormFile? File { get; init; }
}

public sealed class UploadLegalFollowupDocumentHandler
    : IRequestHandler<UploadLegalFollowupDocumentCommand, Result<LegalFollowupDocumentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IFileManager _files;

    public UploadLegalFollowupDocumentHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IFileManager files)
    {
        _db = db;
        _current = current;
        _clock = clock;
        _files = files;
    }

    public async Task<Result<LegalFollowupDocumentDto>> Handle(
        UploadLegalFollowupDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.FieldUpload) == false)
            return new Error("غير مخول برفع مستمسكات المتابعة.", "LegalFollowup.FieldUploadDenied");
        if (_current.UserId is not int followerId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");
        if (!LegalFollowupCompletionRules.IsKnownUploadType(request.DocumentType))
            return new Error("نوع المستمسك غير معروف.", "LegalFollowup.UnknownDocumentType");
        if (request.File is null || request.File.Length <= 0)
            return new Error("الملف مطلوب.", "LegalFollowup.FileRequired");
        if (request.File.Length > LegalFollowupCompletionRules.MaxDocumentBytes)
            return new Error("حجم الملف يتجاوز الحد المسموح.", "LegalFollowup.FileTooLarge");

        var ext = Path.GetExtension(request.File.FileName);
        if (!LegalFollowupCompletionRules.IsAllowedExtension(ext))
            return new Error("نوع الملف غير مسموح.", "LegalFollowup.FileTypeDenied");

        var entity = await _db.LegalFollowupCases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
        if (entity is null || entity.FollowerUserId != followerId)
            return new Error("المتابعة غير موجودة.", "LegalFollowup.NotFound");
        if (entity.Status != LegalFollowupStatus.InProgress)
            return new Error("المتابعة ليست قيد العمل.", "LegalFollowup.NotInProgress");

        var type = request.DocumentType.Trim();
        var existing = await _db.LegalFollowupCaseDocuments
            .FirstOrDefaultAsync(d => d.CaseId == entity.Id && d.DocumentType == type, cancellationToken);
        if (existing is not null)
            _db.LegalFollowupCaseDocuments.Remove(existing);

        var saved = await _files.SaveFileAsync(
            request.File,
            Path.Combine(AttachmentPath.Root, AttachmentPath.LegalFollowupCaseSubfolder(entity.Id)),
            cancellationToken);

        var doc = new LegalFollowupCaseDocument(
            entity.Id,
            type,
            request.File.FileName,
            saved.Name,
            saved.RelativePath,
            request.File.ContentType ?? "application/octet-stream",
            saved.Extension,
            request.File.Length,
            followerId,
            _clock.UtcNowOffset);

        _db.LegalFollowupCaseDocuments.Add(doc);
        await _db.SaveChangesAsync(cancellationToken);
        return LegalFollowupDataService.MapDoc(doc);
    }
}
