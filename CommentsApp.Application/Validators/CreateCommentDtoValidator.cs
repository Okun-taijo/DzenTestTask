using CommentsApp.Application.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .Matches(@"^[\p{L}0-9\s_-]+$")
            .WithMessage("UserName must contain letters, digits, spaces, _ or -");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.HomePage)
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.HomePage))
            .WithMessage("Invalid URL");

        RuleFor(x => x.Text)
            .NotEmpty()
            .Must(BeValidHtml)
            .WithMessage("Only allowed tags: a, code, i, strong");

        RuleFor(x => x.CaptchaCode)
            .NotEmpty();

        RuleFor(x => x.CaptchaId)
            .NotEmpty();
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        if (Uri.TryCreate(url, UriKind.Absolute, out _))
            return true;

        return Uri.TryCreate($"https://{url}", UriKind.Absolute, out _);
    }

    private bool BeValidHtml(string text)
    {
        var tagRegex = new Regex(
            @"^([^<>]|<(\/)?(a|code|i|strong)(\s+(href=""[^""]*""|title=""[^""]*""))*\s*>)*$",
            RegexOptions.IgnoreCase
        );

        return tagRegex.IsMatch(text);
    }
}