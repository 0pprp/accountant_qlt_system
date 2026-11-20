using System.Text.RegularExpressions;
using Domain.Entities.FieldPromoterAggregate;

namespace Application.FieldPromoters.Common;

public static class FieldPromoterErrors
{
    public static readonly Error Forbidden = new("ليس لديك صلاحية لهذه العملية", "FieldPromoter.Forbidden");
    public static readonly Error Suspended = new("تم إيقاف هذا الحساب، يرجى مراجعة الإدارة", "FieldPromoter.Suspended");
    public static readonly Error NoBranch = new("لا يوجد فرع مرتبط بحسابك، راجع الإدارة", "FieldPromoter.NoBranch");
    public static readonly Error BranchForbidden = new("اختر فرعاً من فروعك", "FieldPromoter.BranchForbidden");
    public static readonly Error InvalidName = new("اكتب الاسم الثلاثي بالحروف العربية", "FieldPromoter.InvalidName");
    public static readonly Error InvalidPhone = new("أدخل رقماً عراقياً صحيحاً مثل 07701234567", "FieldPromoter.InvalidPhone");
    public static readonly Error InvalidProduct = new("اختر المنتج أو اكتب اسمه", "FieldPromoter.InvalidProduct");
    public static readonly Error InvalidAddress = new("اكتب عنوان الزبون", "FieldPromoter.InvalidAddress");
    public static readonly Error InvalidNote = new("الملاحظة أطول من المسموح", "FieldPromoter.InvalidNote");
    public static readonly Error OutsideIraq = new("الموقع خارج حدود العراق", "FieldPromoter.OutsideIraq");
    public static readonly Error InvalidPhoto = new("الصورة يجب أن تكون JPG أو PNG بحجم لا يتجاوز 2 ميغابايت", "FieldPromoter.InvalidPhoto");
    public static readonly Error DuplicateOpenOrder = new("يوجد طلب مفتوح لنفس الزبون ونفس المنتج", "FieldPromoter.DuplicateOpenOrder");
    public static readonly Error OrderNotFound = new("الطلب غير موجود", "FieldPromoter.OrderNotFound");
    public static readonly Error CannotCancel = new("لا يمكن إلغاء الطلب بعد إسناده إلى موظف المبيعات", "FieldPromoter.CannotCancel");
    public static readonly Error InvalidPinName = new("اكتب اسم المحل (حرفان على الأقل)", "FieldPromoter.InvalidPinName");
    public static readonly Error InvalidInterest = new("اختر مدى اهتمام صاحب المحل", "FieldPromoter.InvalidInterest");
    public static readonly Error PinNotFound = new("المحل غير موجود", "FieldPromoter.PinNotFound");
    public static readonly Error NotOwner = new("يمكنك تعديل محلاتك فقط", "FieldPromoter.NotOwner");
    public static readonly Error NoActiveShift = new("لا يوجد دوام مفتوح", "FieldPromoter.NoActiveShift");
    public static readonly Error BatchTooLarge = new("عدد النقاط أكبر من الحد المسموح", "FieldPromoter.BatchTooLarge");
    public static readonly Error PromoterNotFound = new("المروّج غير موجود", "FieldPromoter.PromoterNotFound");
    public static readonly Error UsernameTaken = new("اسم المستخدم مستعمل", "FieldPromoter.UsernameTaken");
    public static readonly Error InvalidAccount = new("تحقق من بيانات الحساب", "FieldPromoter.InvalidAccount");
    public static readonly Error InvalidRange = new("مدى التاريخ غير صالح (93 يوماً كحد أقصى)", "FieldPromoter.InvalidRange");
    public static readonly Error InvalidSettings = new("تحقق من قيم الإعدادات", "FieldPromoter.InvalidSettings");
    public static readonly Error AlertNotFound = new("التنبيه غير موجود", "FieldPromoter.AlertNotFound");
}

public static class FieldPromoterRules
{
    public static readonly TimeSpan IraqOffset = TimeSpan.FromHours(3);
    public static readonly TimeSpan PeriodGap = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan QuickVisit = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan StationaryFreshness = TimeSpan.FromMinutes(20);
    public const int MaxBatchPoints = 500;
    public const int MaxDayPoints = 5000;
    public const int MaxBoxPins = 500;
    public const int MaxRangeDays = 93;
    public const int LocationRetentionDays = 90;
    public const int MaxProductLength = 100;
    public const int MaxAddressLength = 400;
    public const int MaxNoteLength = 500;
    public const int MaxNameLength = 150;

    public static readonly string[] Products = ["ايفون", "دراجة", "كرسي مساج", "شاشة"];

    public static readonly string[] InterestLabels =
        ["غير مهتم إطلاقاً", "قليل الاهتمام", "مهتم نوعاً ما", "مهتم", "مهتم جداً"];

    public static string InterestLabel(byte interest) =>
        interest is >= 1 and <= 5 ? InterestLabels[interest - 1] : "";

    public static DateOnly IraqDay(DateTimeOffset moment) =>
        DateOnly.FromDateTime(moment.ToOffset(IraqOffset).DateTime);

    public static (DateTimeOffset Start, DateTimeOffset End) DayBounds(DateOnly day)
    {
        var start = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), IraqOffset).ToUniversalTime();
        return (start, start.AddDays(1));
    }

    public static bool InsideIraq(double lat, double lng) =>
        double.IsFinite(lat) && double.IsFinite(lng) && lat is >= 29 and <= 38 && lng is >= 38 and <= 49;

    public static string ToWesternDigits(string? value)
    {
        var mapped = (value ?? "").Select(c =>
            c >= '٠' && c <= '٩' ? (char)('0' + c - '٠') :
            c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c);
        return new string(mapped.ToArray());
    }

    /// <summary>هاتف عراقي بصيغة 07XXXXXXXXX، يقبل +964 و00964 و964 وغياب الصفر.</summary>
    public static string? NormalizePhone(string? value)
    {
        var digits = new string(ToWesternDigits(value).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00964")) digits = "0" + digits[5..];
        else if (digits.StartsWith("964")) digits = "0" + digits[3..];
        else if (digits.Length == 10 && digits.StartsWith('7')) digits = "0" + digits;
        return Regex.IsMatch(digits, @"^07[3-9]\d{8}$") ? digits : null;
    }

    private static readonly Regex ArabicWord = new(@"^[ء-يٱ-ۓ]{2,30}$", RegexOptions.Compiled);
    private static readonly string[] JoinWithNext = ["عبد", "ابو", "أبو", "ابي", "أبي"];

    /// <summary>الاسم الثلاثي العربي: «عبد/ابو + الكلمة التالية» تُحسب اسماً واحداً.</summary>
    public static string? NormalizeTripleName(string? value)
    {
        var cleaned = Regex.Replace(value ?? "", @"[ً-ٰٟـ​-‏]", "");
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var parts = new List<string>();
        for (var i = 0; i < words.Count; i++)
        {
            if (JoinWithNext.Contains(words[i]) && i + 1 < words.Count)
            {
                parts.Add(words[i] + " " + words[i + 1]);
                i++;
                continue;
            }
            parts.Add(words[i]);
        }

        if (parts.Count < 3)
            return null;
        if (words.Any(w => ArabicWord.IsMatch(w) == false && JoinWithNext.Contains(w) == false))
            return null;
        var joined = string.Join(' ', words);
        return joined.Length <= MaxNameLength ? joined : null;
    }

    public static string? NormalizeProduct(string? value)
    {
        var v = Regex.Replace((value ?? "").Trim(), @"\s+", " ");
        return v.Length is >= 2 and <= MaxProductLength ? v : null;
    }

    public static double DistanceMeters(double lat1, double lng1, double lat2, double lng2)
    {
        const double r = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 2 * r * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    public static FieldPromoterDayStatus DayStatus(int targetMinutes, int workMinutes, bool worked)
    {
        if (targetMinutes <= 0) return FieldPromoterDayStatus.NoTarget;
        if (workMinutes >= targetMinutes) return FieldPromoterDayStatus.Met;
        return worked ? FieldPromoterDayStatus.Missed : FieldPromoterDayStatus.Off;
    }

    public static string DayStatusLabel(FieldPromoterDayStatus status) => status switch
    {
        FieldPromoterDayStatus.Met => "حقّق",
        FieldPromoterDayStatus.Missed => "لم يحقّق",
        FieldPromoterDayStatus.Off => "غائب",
        _ => "بلا هدف"
    };
}

public sealed record WorkPeriod(DateTimeOffset Start, DateTimeOffset End, int Events)
{
    public int Minutes => (int)Math.Round((End - Start).TotalMinutes);
}

/// <summary>
/// فترات العمل من أحداث اليوم (تثبيت محل أو إرسال طلب): فجوة 15 دقيقة أو أكثر تقطع الفترة،
/// ودقائق اليوم مجموع أطوال الفترات.
/// </summary>
public static class WorkPeriods
{
    public static List<WorkPeriod> From(IEnumerable<DateTimeOffset> events)
    {
        var ordered = events.OrderBy(x => x).ToList();
        var periods = new List<WorkPeriod>();
        if (ordered.Count == 0)
            return periods;

        var start = ordered[0];
        var last = ordered[0];
        var count = 1;
        foreach (var moment in ordered.Skip(1))
        {
            if (moment - last >= FieldPromoterRules.PeriodGap)
            {
                periods.Add(new WorkPeriod(start, last, count));
                start = moment;
                count = 0;
            }
            last = moment;
            count++;
        }
        periods.Add(new WorkPeriod(start, last, count));
        return periods;
    }

    public static int TotalMinutes(IEnumerable<WorkPeriod> periods) => periods.Sum(p => p.Minutes);
}

public sealed record LocationSample(double Latitude, double Longitude, DateTimeOffset At);

public sealed record StationaryStay(double Latitude, double Longitude, DateTimeOffset Since, DateTimeOffset Until);

/// <summary>
/// الوقوف الطويل: أحدث نقطة حديثة (20 دقيقة)، والوقوف هو سلسلة النقاط المتصلة ضمن نصف القطر حول أحدث نقطة.
/// </summary>
public static class StationaryDetector
{
    public static StationaryStay? Detect(IReadOnlyList<LocationSample> samples, int radiusMeters, int minutes,
        DateTimeOffset now)
    {
        if (samples.Count == 0)
            return null;
        var newestFirst = samples.OrderByDescending(s => s.At).ToList();
        var newest = newestFirst[0];
        if (now - newest.At > FieldPromoterRules.StationaryFreshness)
            return null;

        var stay = new List<LocationSample>();
        foreach (var s in newestFirst)
        {
            if (FieldPromoterRules.DistanceMeters(newest.Latitude, newest.Longitude, s.Latitude, s.Longitude) > radiusMeters)
                break;
            stay.Add(s);
        }

        var since = stay[^1].At;
        if ((newest.At - since).TotalMinutes < minutes)
            return null;
        return new StationaryStay(stay.Average(s => s.Latitude), stay.Average(s => s.Longitude), since, newest.At);
    }
}

/// <summary>يقبل JPEG وPNG فقط ويحذف بيانات EXIF (ومنها موقع الهاتف) قبل الحفظ.</summary>
public static class ShopPhotoSanitizer
{
    public static (byte[] Bytes, string ContentType)? Sanitize(byte[] data)
    {
        if (data.Length is 0 or > FieldPromoterOrderShop.MaxPhotoBytes)
            return null;
        if (data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            var clean = StripJpegMetadata(data);
            return clean is null ? null : (clean, "image/jpeg");
        }
        if (data.Length > 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
        {
            var clean = StripPngMetadata(data);
            return clean is null ? null : (clean, "image/png");
        }
        return null;
    }

    private static byte[]? StripJpegMetadata(byte[] data)
    {
        using var output = new MemoryStream(data.Length);
        output.WriteByte(0xFF);
        output.WriteByte(0xD8);
        var i = 2;
        while (i + 4 <= data.Length)
        {
            if (data[i] != 0xFF)
                return null;
            var marker = data[i + 1];
            if (marker == 0xDA)
            {
                output.Write(data, i, data.Length - i);
                return output.ToArray();
            }
            var length = (data[i + 2] << 8) | data[i + 3];
            if (length < 2 || i + 2 + length > data.Length)
                return null;
            var isMetadata = marker == 0xE1 || (marker >= 0xE2 && marker <= 0xEF) || marker == 0xFE;
            if (isMetadata == false)
                output.Write(data, i, 2 + length);
            i += 2 + length;
        }
        return null;
    }

    private static byte[]? StripPngMetadata(byte[] data)
    {
        string[] drop = ["eXIf", "tEXt", "iTXt", "zTXt", "tIME"];
        using var output = new MemoryStream(data.Length);
        output.Write(data, 0, 8);
        var i = 8;
        while (i + 12 <= data.Length)
        {
            var length = (data[i] << 24) | (data[i + 1] << 16) | (data[i + 2] << 8) | data[i + 3];
            if (length < 0 || i + 12 + length > data.Length)
                return null;
            var type = System.Text.Encoding.ASCII.GetString(data, i + 4, 4);
            if (drop.Contains(type) == false)
                output.Write(data, i, 12 + length);
            i += 12 + length;
            if (type == "IEND")
                return output.ToArray();
        }
        return null;
    }
}
