namespace CommentsApp.Application.Interfaces;

public interface ITextSanitizer
{
    string Sanitize(string input);
}