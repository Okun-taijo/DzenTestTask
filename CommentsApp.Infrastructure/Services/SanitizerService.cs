using CommentsApp.Application.Interfaces;
using Ganss.Xss;
namespace CommentsApp.Infrastructure.Services;

public class SanitizerService: ITextSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public SanitizerService()
    {
         _sanitizer = new HtmlSanitizer();

        _sanitizer.AllowedTags.Clear();

        _sanitizer.AllowedTags.Add("a");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("strong");

        _sanitizer.AllowedAttributes.Clear();

        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.RemovingAttribute += (s, e) =>
        {
            if (e.Tag.TagName != "a" &&
                (e.Attribute.Name == "href" || e.Attribute.Name == "title"))
            {
                e.Cancel = false;
            }
        };

        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");
    }

    public string Sanitize(string input)
    {
        return _sanitizer.Sanitize(input);
    }
}
