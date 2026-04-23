using CommentsApp.Application.DTOs;
using CommentsApp.Application.Interfaces;
using CommentsApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommentsApp.API.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly CommentService _commentService;
    private readonly ICaptchaService _captchaService;

    public CommentsController(CommentService commentService, ICaptchaService captchaService)
    {
        _commentService = commentService;
        _captchaService = captchaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCommentDto dto)
    {
        if (!_captchaService.Validate(dto.CaptchaId, dto.CaptchaCode))
            return BadRequest(new
            {
                message = "Invalid captcha"
            });

        await _commentService.CreateAsync(dto);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        int page = 1,
        string sortBy = "date",
        bool asc = false)
    {
        const int pageSize = 25;

        var result = await _commentService.GetAllAsync(page, pageSize, sortBy, asc);

        return Ok(result);
    }

}