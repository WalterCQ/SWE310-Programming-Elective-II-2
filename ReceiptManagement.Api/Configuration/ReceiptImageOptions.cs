namespace ReceiptManagement.Api.Configuration;

public class ReceiptImageOptions
{
    public const string SectionName = "ReceiptImages";
    public const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;

    public long MaxFileSizeBytes { get; set; } = DefaultMaxFileSizeBytes;
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
