namespace CommentsApp.Application.Models;


public class CommentAttachment
{
    public int Id {get; set;}

    public int CommentId {get; set;}
    public Comment Comment {get; set;}=null!;

    public string FileName {get;set;}=null!;
    public string Path {get; set;}=null!;
    public long Size {get; set;}
    public string ContentType {get; set;}=null!;

    public bool IsImage {get; set;}
}