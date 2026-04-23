using CommentsApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IElasticService _elasticService;
    private readonly ICommentRepository _commentRepository;

    public SearchController(
        IElasticService elasticService,
        ICommentRepository commentRepository)
    {
        _elasticService = elasticService;
        _commentRepository = commentRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<CommentSearchResult>());

        var searchResults = await _elasticService.Search(q);
        var ids = searchResults.Select(x => x.Id).Distinct().ToList();
        var comments = await _commentRepository.GetByIdsFlatAsync(ids);

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var commentsById = comments.ToDictionary(x => x.Id);

        var ordered = ids
            .Where(commentsById.ContainsKey)
            .Select(id => commentsById[id])
            .ToList();

        var response = ordered.Select(c => new
        {
            c.Id,
            c.UserName,
            c.Email,
            c.HomePage,
            c.Text,
            c.CreatedAt,
            Attachments = c.Attachments.Select(a => new
            {
                a.Id,
                a.FileName,
                Path = $"{baseUrl}/uploads/{a.Path}",
                a.IsImage
            }).ToList()
        });

        return Ok(response);
    }
}