using CommentsApp.Application.DTOs;
namespace CommentsApp.Application.Interfaces;

public interface ICaptchaService
{
    CaptchaResult Generate();
    bool Validate(string captchaId, string code);
}