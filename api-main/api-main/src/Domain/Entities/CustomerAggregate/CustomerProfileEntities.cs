using Domain.Common;

namespace Domain.Entities.CustomerAggregate;

/// <summary>مصدر إحداثيات الزبون — الملتقط يدوياً أدقّ من المُقدَّر من تجمّع نقاط المندوب.</summary>
public enum CustomerLocationSource : byte
{
    Captured = 0,
    Estimated = 1,
    ShopProfile = 2,
    LegalCase = 3
}

/// <summary>موقع محل الزبون — قد يكون له أكثر من مصدر، والأدقّ يُرجَّح.</summary>
public class CustomerLocation : Entity
{
    private CustomerLocation()
    {
    }

    public CustomerLocation(
        int customerId,
        CustomerLocationSource source,
        double latitude,
        double longitude,
        double? accuracyMeters,
        double? confidence,
        DateTimeOffset? capturedAtUtc)
    {
        CustomerId = customerId;
        Source = source;
        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        Confidence = confidence;
        CapturedAtUtc = capturedAtUtc;
    }

    public int CustomerId { get; private set; }
    public CustomerLocationSource Source { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double? AccuracyMeters { get; private set; }
    public double? Confidence { get; private set; }
    public DateTimeOffset? CapturedAtUtc { get; private set; }
    public string? ImportKey { get; set; }
}

/// <summary>صورة محل الزبون — محفوظة في القاعدة كما في النظام المصدر.</summary>
public class CustomerShopPhoto : Entity
{
    public const int MaxContentTypeLength = 100;

    private CustomerShopPhoto()
    {
    }

    public CustomerShopPhoto(
        int customerId,
        byte[] content,
        string contentType,
        DateTimeOffset? capturedAtUtc)
    {
        CustomerId = customerId;
        Content = content;
        ContentType = contentType;
        CapturedAtUtc = capturedAtUtc;
    }

    public int CustomerId { get; private set; }
    public byte[] Content { get; private set; } = [];
    public string ContentType { get; private set; } = "image/jpeg";
    public DateTimeOffset? CapturedAtUtc { get; private set; }
    public string? ImportKey { get; set; }
}

/// <summary>مستند زبون — المفتاح مسار نسبي داخل مخزن الملفات لا ملفاً في القاعدة.</summary>
public class CustomerDocument : Entity
{
    public const int MaxKeyLength = 1000;
    public const int MaxNameLength = 300;

    private CustomerDocument()
    {
    }

    public CustomerDocument(
        int customerId,
        int? orderId,
        string documentType,
        string fileKey,
        string? fileName,
        string? contentType,
        DateTimeOffset? createdAtUtc)
    {
        CustomerId = customerId;
        OrderId = orderId;
        DocumentType = documentType;
        FileKey = fileKey;
        FileName = fileName;
        ContentType = contentType;
        CreatedAtUtc = createdAtUtc;
    }

    public int CustomerId { get; private set; }
    public int? OrderId { get; private set; }
    public string DocumentType { get; private set; } = "";
    public string FileKey { get; private set; } = "";
    public string? FileName { get; private set; }
    public string? ContentType { get; private set; }
    public DateTimeOffset? CreatedAtUtc { get; private set; }
    public string? ImportKey { get; set; }
}

/// <summary>لقطة تقييم محفوظة — تقييم تاريخي بتاريخه لا يُعاد حسابه.</summary>
public class CustomerRatingSnapshot : Entity
{
    public const int MaxLabelLength = 40;

    private CustomerRatingSnapshot()
    {
    }

    public CustomerRatingSnapshot(
        int customerId,
        string origin,
        string? ratingLabel,
        double? score,
        DateOnly captureDate,
        DateTimeOffset? capturedAtUtc)
    {
        CustomerId = customerId;
        Origin = origin;
        RatingLabel = ratingLabel;
        Score = score;
        CaptureDate = captureDate;
        CapturedAtUtc = capturedAtUtc;
    }

    public const string OriginBranchManager = "BranchManager";
    public const string OriginFollower = "Follower";

    public int CustomerId { get; private set; }
    public string Origin { get; private set; } = "";
    public string? RatingLabel { get; private set; }
    public double? Score { get; private set; }
    public DateOnly CaptureDate { get; private set; }
    public DateTimeOffset? CapturedAtUtc { get; private set; }
    public string? ImportKey { get; set; }
}

/// <summary>بروفايل محل الزبون — مساحة ومخزون وإيراد يومي مقدّر.</summary>
public class CustomerShopProfile : Entity
{
    public const int MaxNameLength = 200;

    private CustomerShopProfile()
    {
    }

    public CustomerShopProfile(
        int customerId,
        int? orderId,
        string? shopName,
        string? businessType,
        double? stockEstimatedValue,
        double? estimatedDailyRevenue,
        double? shopLength,
        double? shopWidth,
        double? shopArea,
        string? shopImageKey,
        DateTimeOffset? createdAtUtc)
    {
        CustomerId = customerId;
        OrderId = orderId;
        ShopName = shopName;
        BusinessType = businessType;
        StockEstimatedValue = stockEstimatedValue;
        EstimatedDailyRevenue = estimatedDailyRevenue;
        ShopLength = shopLength;
        ShopWidth = shopWidth;
        ShopArea = shopArea;
        ShopImageKey = shopImageKey;
        CreatedAtUtc = createdAtUtc;
    }

    public int CustomerId { get; private set; }
    public int? OrderId { get; private set; }
    public string? ShopName { get; private set; }
    public string? BusinessType { get; private set; }
    public double? StockEstimatedValue { get; private set; }
    public double? EstimatedDailyRevenue { get; private set; }
    public double? ShopLength { get; private set; }
    public double? ShopWidth { get; private set; }
    public double? ShopArea { get; private set; }
    public string? ShopImageKey { get; private set; }
    public DateTimeOffset? CreatedAtUtc { get; private set; }
    public string? ImportKey { get; set; }
}
