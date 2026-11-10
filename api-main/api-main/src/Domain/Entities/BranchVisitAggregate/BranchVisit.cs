using Domain.Common;

namespace Domain.Entities.BranchVisitAggregate;

public class BranchVisit : Entity
{
    private readonly List<BranchVisitRating> _ratings = [];
    private readonly List<BranchVisitMember> _members = [];
    private readonly List<BranchVisitAttachment> _attachments = [];

    private BranchVisit() { }

    public BranchVisit(int branchId, int openedByUserId, string openedByName, DateTimeOffset at)
    {
        BranchId = branchId;
        OpenedByUserId = openedByUserId;
        OpenedByName = openedByName;
        ArrivedAtUtc = at;
        Status = BranchVisitStatus.Open;
        ConcurrencyStamp = 0;
    }

    public int BranchId { get; private set; }
    public int OpenedByUserId { get; private set; }
    public string OpenedByName { get; private set; } = "";
    public DateTimeOffset ArrivedAtUtc { get; private set; }
    public DateTimeOffset? DepartedAtUtc { get; private set; }
    public BranchVisitStatus Status { get; private set; }
    public string? DepartNote { get; private set; }
    public int ConcurrencyStamp { get; private set; }
    public IReadOnlyCollection<BranchVisitRating> Ratings => _ratings;
    public IReadOnlyCollection<BranchVisitMember> Members => _members;
    public IReadOnlyCollection<BranchVisitAttachment> Attachments => _attachments;

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    /// <summary>لقطة أعضاء عند الفتح فقط — لا تُعاد كتابتها عند إعادة الفتح idempotent.</summary>
    public void SetMemberSnapshot(IEnumerable<(int UserId, string DisplayName)> members)
    {
        if (_members.Count > 0) return;
        foreach (var m in members)
        {
            if (m.UserId <= 0) continue;
            var name = string.IsNullOrWhiteSpace(m.DisplayName) ? $"#{m.UserId}" : m.DisplayName.Trim();
            _members.Add(new BranchVisitMember(m.UserId, name));
        }
    }

    public void UpsertRating(int orderListId, string listName, int score, int actorId, DateTimeOffset at, int expectedVersion)
    {
        if (Status != BranchVisitStatus.Open)
            throw new InvalidOperationException("الزيارة ليست مفتوحة.");
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (score is < 0 or > 10)
            throw new InvalidOperationException("التقييم بين 0 و10.");
        var existing = _ratings.FirstOrDefault(r => r.OrderListId == orderListId);
        if (existing is null)
            _ratings.Add(new BranchVisitRating(orderListId, listName, score, actorId, at));
        else
            existing.Update(score, actorId, at, listName);
        ConcurrencyStamp++;
    }

    public BranchVisitAttachment AddAttachment(
        string originalFileName,
        string storedFileName,
        string relativePath,
        string contentType,
        string extension,
        long sizeBytes,
        string contentSha256,
        int uploadedByUserId,
        DateTimeOffset at)
    {
        if (Status != BranchVisitStatus.Open)
            throw new InvalidOperationException("الزيارة ليست مفتوحة.");
        if (_attachments.Any(a => a.ContentSha256 == contentSha256))
            return _attachments.First(a => a.ContentSha256 == contentSha256);

        var att = new BranchVisitAttachment(
            originalFileName, storedFileName, relativePath, contentType, extension,
            sizeBytes, contentSha256, uploadedByUserId, at);
        _attachments.Add(att);
        return att;
    }

    public void Depart(string note, IEnumerable<int> requiredListIds, int actorId, DateTimeOffset at, int expectedVersion)
    {
        if (Status == BranchVisitStatus.Departed && OpenedByUserId == actorId)
            return; // idempotent re-depart
        if (Status != BranchVisitStatus.Open)
            throw new InvalidOperationException("الزيارة ليست مفتوحة.");
        if (OpenedByUserId != actorId)
            throw new InvalidOperationException("فقط من فتح الزيارة يغادر.");
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        var t = (note ?? "").Trim();
        if (t.Length < 50)
            throw new InvalidOperationException("ملاحظة المغادرة 50 حرفًا على الأقل.");
        var required = requiredListIds.Where(id => id > 0).Distinct().ToHashSet();
        var rated = _ratings.Select(r => r.OrderListId).ToHashSet();
        if (required.Count > 0)
        {
            if (required.Any(id => rated.Contains(id) == false))
                throw new InvalidOperationException("يجب تقييم كل القوائم المطلوبة قبل المغادرة.");
        }
        else if (rated.Count == 0)
        {
            throw new InvalidOperationException("يجب تقييم المندوبين قبل مغادرة الفرع.");
        }
        DepartNote = t;
        DepartedAtUtc = at;
        Status = BranchVisitStatus.Departed;
        ConcurrencyStamp++;
    }
}

public class BranchVisitMember : Entity
{
    private BranchVisitMember() { }
    public BranchVisitMember(int userId, string displayName)
    {
        UserId = userId;
        DisplayName = displayName;
    }
    public int VisitId { get; private set; }
    public int UserId { get; private set; }
    public string DisplayName { get; private set; } = "";
}

public class BranchVisitRating : Entity
{
    private BranchVisitRating() { }
    public BranchVisitRating(int orderListId, string listName, int score, int ratedByUserId, DateTimeOffset at)
    {
        OrderListId = orderListId;
        ListName = listName;
        Score = score;
        RatedByUserId = ratedByUserId;
        RatedAtUtc = at;
    }
    public int VisitId { get; private set; }
    public int OrderListId { get; private set; }
    public string ListName { get; private set; } = "";
    public int Score { get; private set; }
    public int RatedByUserId { get; private set; }
    public DateTimeOffset RatedAtUtc { get; private set; }
    public void Update(int score, int actorId, DateTimeOffset at, string listName)
    {
        Score = score;
        RatedByUserId = actorId;
        RatedAtUtc = at;
        ListName = listName;
    }
}

public class BranchVisitAttachment : Entity
{
    private BranchVisitAttachment() { }
    public BranchVisitAttachment(
        string originalFileName,
        string storedFileName,
        string relativePath,
        string contentType,
        string extension,
        long sizeBytes,
        string contentSha256,
        int uploadedByUserId,
        DateTimeOffset at)
    {
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        RelativePath = relativePath;
        ContentType = contentType;
        Extension = extension;
        SizeBytes = sizeBytes;
        ContentSha256 = contentSha256;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = at;
    }
    public int VisitId { get; private set; }
    public string OriginalFileName { get; private set; } = "";
    public string StoredFileName { get; private set; } = "";
    public string RelativePath { get; private set; } = "";
    public string ContentType { get; private set; } = "";
    public string Extension { get; private set; } = "";
    public long SizeBytes { get; private set; }
    public string ContentSha256 { get; private set; } = "";
    public int UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public enum BranchVisitStatus : byte { Open = 1, Departed = 2 }

public static class BranchVisitFileRules
{
    public const long MaxBytes = 20 * 1024 * 1024;
    /// <summary>
    /// المصدر لا يفرض عدد ملفات صريحًا؛ سقف الطلب HTTP 25MiB هو القيد العملي.
    /// هذا الحد الاحترازي يمنع إغراق الذاكرة فقط وليس عقد مصدر.
    /// </summary>
    public const int MaxFilesPerDepart = 40;
    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf"
    };

    public static bool TryDetectKind(ReadOnlySpan<byte> header, string extension, out string contentType)
    {
        contentType = "";
        var ext = (extension ?? "").ToLowerInvariant();
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF
            && (ext is ".jpg" or ".jpeg"))
        {
            contentType = "image/jpeg";
            return true;
        }
        if (header.Length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && ext == ".png")
        {
            contentType = "image/png";
            return true;
        }
        if (header.Length >= 6 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && ext == ".gif")
        {
            contentType = "image/gif";
            return true;
        }
        if (header.Length >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50
            && ext == ".webp")
        {
            contentType = "image/webp";
            return true;
        }
        if (header.Length >= 5 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46
            && ext == ".pdf")
        {
            contentType = "application/pdf";
            return true;
        }
        return false;
    }
}
