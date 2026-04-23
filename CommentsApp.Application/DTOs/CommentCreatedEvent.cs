namespace CommentsApp.Application.DTOs;

public class CommentCreatedEvent
{
    public string Type { get; set; }=null!;
    public int CommentId { get; set; }
    public string Text { get; set; }=null!;
    public string UserName { get; set; }=null!;
}