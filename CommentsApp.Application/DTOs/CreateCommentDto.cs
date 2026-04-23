using Microsoft.AspNetCore.Http;

namespace CommentsApp.Application.DTOs;

public class CreateCommentDto
{
    public string UserName {get; set;} = null!;
    public string Email {get; set;} = null!;
    public string? HomePage{get; set;}
    public string Text{get; set;} = null!;
    public int? ParentId{get;set;}

    public string CaptchaCode { get; set; } = null!;
    public string CaptchaId { get; set; } = null!;
    public List<IFormFile>? Files { get; set; }
}