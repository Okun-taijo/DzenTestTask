namespace CommentsApp.Application.DTOs;
public class CommentAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public bool IsImage { get; set; }
}