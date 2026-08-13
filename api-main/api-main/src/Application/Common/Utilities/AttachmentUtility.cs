using Domain.Entities.AttachmentAggregate.Enums;

namespace Application.Common.Utilities;

public static class AttachmentUtility
{
    static AttachmentUtility()
    {
        ValidContentTypes.AddRange(ImageContentTypes);
        ValidContentTypes.AddRange(VideoContentTypes);
        ValidContentTypes.AddRange(IconContentTypes);
        
        CertificateContentTypes.AddRange(ImageContentTypes);
        CertificateContentTypes.AddRange(FileContentTypes);
    }
    
    public static readonly List<string> ValidContentTypes = [];
    public static readonly List<string> CertificateContentTypes = [];

    public static readonly List<string> ImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    ];

    public static readonly List<string> VideoContentTypes =
    [
        "video/mp4",
        "video/mpeg",
        "video/x-msvideo",
        "video/ogg"
    ];
    
    public static readonly List<string> IconContentTypes =
    [
        "image/png",
        "image/svg+xml"
    ];
    
    public static readonly List<string> AudioContentTypes =
    [
        "audio/mpeg", // mp3
        "audio/vorbis",
        "audio/aac",
        "audio/ogg",
        "audio/wav",
        "audio/x-wav"
    ];
    
    public static readonly List<string> FileContentTypes =
    [
        "text/plain", // txt
        "application/zip", // Zip
        "application/x-zip-compressed", // Zip
        "application/vnd.rar", // Rar
        "application/msword", // microsoft word
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // microsoft word
        "application/pdf", // pdf
        "application/vnd.ms-excel", // microsoft excel
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // microsoft excel
        "application/vnd.ms-powerpoint", // microsoft powerpoint
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", // microsoft powerpoint
    ];
    
    public static FileType GetFileType(string contentType)
    {
        if (ImageContentTypes.Any(x => x == contentType))
            return FileType.Picture;

        if (VideoContentTypes.Any(x => x == contentType))
            return FileType.Video;
        
        if (AudioContentTypes.Any(x => x == contentType))
            return FileType.Audio;
        
        if (FileContentTypes.Any(x => x == contentType))
            return FileType.Document;

        throw new ArgumentException("content type is invalid");
    }
}