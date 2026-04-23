using System.Text.Json;
using CommentsApp.Application.DTOs;
using CommentsApp.Application.Interfaces;
using CommentsApp.Application.Models;
using Microsoft.AspNetCore.Http;

namespace CommentsApp.Application.Services;
public class CommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITextSanitizer _textSanitizer;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;
    private readonly IMessageBus _messageBus;
    private readonly IRealtimeNotifier _notifier;

    public CommentService(
        ICommentRepository commentRepository,
        ITextSanitizer textSanitizer,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        ICacheService cacheService,
        IMessageBus messageBus,
        IRealtimeNotifier notifier)
    {
        _commentRepository = commentRepository;
        _textSanitizer = textSanitizer;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
        _cacheService = cacheService;
        _messageBus = messageBus;
        _notifier = notifier;
    }

    public async Task CreateAsync(CreateCommentDto commentDto)
    {
        var text = _textSanitizer.Sanitize(commentDto.Text);

        if (string.IsNullOrWhiteSpace(text))
            throw new Exception("Text is empty after sanitization");

        var comment = new Comment
        {
            UserName = commentDto.UserName,
            Email = commentDto.Email,
            HomePage = commentDto.HomePage, 
            Text = text,
            ParentId = commentDto.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var file in commentDto.Files ?? Enumerable.Empty<IFormFile>())
        {
            await _fileService.ProcessAsync(file, comment);
        }
        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        await _messageBus.Publish(JsonSerializer.Serialize(new
        {
            Type = "CommentCreated",
            CommentId = comment.Id,
            Text = comment.Text,
            UserName = comment.UserName
        }));
        
        await _notifier.NewComment(new
        {
            comment.Id,
            comment.UserName,
            comment.Email,
            comment.HomePage,
            comment.Text,
            comment.CreatedAt,
            comment.ParentId,
            Attachments = comment.Attachments.Select(a => new
            {
                a.Id,
                a.FileName,
                Path = $"/uploads/{NormalizeStoredFileName(a.Path)}",
                a.IsImage
            }).ToList()
        });
    }

    public async Task<List<CommentDto>> GetAllAsync(
        int page,
        int pageSize,
        string? sortBy,
        bool asc)
    {
        var cacheKey = $"comments_page_{page}_{sortBy ?? "date"}_{asc}";

        var cached = await _cacheService.GetAsync<List<CommentDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var allComments = await _commentRepository.GetAllFlatAsync();
        var tree = BuildTree(allComments);
        tree = ApplySorting(tree, sortBy, asc);

        var result = tree
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;  
    }

    private List<CommentDto> BuildTree(List<Comment> comments)
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var dict = comments.ToDictionary(
            x => x.Id,
            x => new CommentDto
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                HomePage = x.HomePage,
                Text = x.Text,
                CreatedAt = x.CreatedAt,
                Replies = new List<CommentDto>(),
                Attachments = x.Attachments.Select(a => new CommentAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    Path = $"{baseUrl}/uploads/{NormalizeStoredFileName(a.Path)}",
                    IsImage = a.IsImage
                }).ToList()
            }
        );

        var roots = new List<CommentDto>();

        foreach (var c in comments)
        {
            var node = dict[c.Id];

            if (c.ParentId == null || !dict.ContainsKey(c.ParentId.Value))
            {
                roots.Add(node);
            }
            else
            {
                dict[c.ParentId.Value].Replies.Add(node);
            }
        }

        return roots;
    }

    private List<CommentDto> ApplySorting(
        List<CommentDto> tree,
        string? sortBy,
        bool asc)
    {
        return sortBy switch
        {
            "username" => asc
                ? tree.OrderBy(x => x.UserName).ToList()
                : tree.OrderByDescending(x => x.UserName).ToList(),

            "email" => asc
                ? tree.OrderBy(x => x.Email).ToList()
                : tree.OrderByDescending(x => x.Email).ToList(),

            _ => asc
                ? tree.OrderBy(x => x.CreatedAt).ToList()
                : tree.OrderByDescending(x => x.CreatedAt).ToList(),
        };
    }

    private static string NormalizeStoredFileName(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            return string.Empty;

        var normalized = rawPath.Replace('\\', '/').Trim();
        if (normalized.Contains('/'))
            return global::System.IO.Path.GetFileName(normalized);

        return normalized;
    }

}