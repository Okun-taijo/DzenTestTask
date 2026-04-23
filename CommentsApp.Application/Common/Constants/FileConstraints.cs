namespace CommentsApp.Application.Common;
public static class FileConstraints
{
    public static readonly string[] AllowedImageTypes =
    {
        "image/jpeg",
        "image/png",
        "image/gif"
    };

    public const int MaxImageWidth = 320;
    public const int MaxImageHeight = 240;

    public const int MaxTextFileSize = 100 * 1024;
}