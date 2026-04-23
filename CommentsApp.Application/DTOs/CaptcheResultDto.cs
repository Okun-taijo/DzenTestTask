namespace CommentsApp.Application.DTOs;

public class CaptchaResult
{
    public string CaptchaId { get; set; } = null!;
    public string ImageBase64 { get; set; } = null!;
}