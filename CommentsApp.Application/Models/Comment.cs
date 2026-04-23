using System.Data;

namespace CommentsApp.Application.Models;

public class Comment
{
    public int Id {get;set;}

    public string UserName {get; set;} = null!;
    public string Email {get; set;} = null!;
    public string? HomePage{get; set;}

    public string Text{get; set;} = null!;

    public int? ParentId{get;set;}
    public Comment? Parent {get; set;}
    public List<Comment> Replies {get;set;} = new();
    public List<CommentAttachment> Attachments { get; set; } = new();

    public DateTime CreatedAt{get;set;}
}