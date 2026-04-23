using CommentsApp.Application.Models;

namespace CommentsApp.Application.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? HomePage { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public List<CommentAttachmentDto> Attachments { get; set; } = new();
    public List<CommentDto> Replies { get; set; } = new();
}