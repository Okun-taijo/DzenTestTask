using CommentsApp.Application.DTOs;
using CommentsApp.Application.Services;

namespace CommentsApp.API.GraphQL;

public class CommentsQuery
{
    public Task<List<CommentDto>> Comments(
        [Service] CommentService commentService,
        int page = 1,
        int pageSize = 25,
        string sortBy = "date",
        bool asc = false)
    {
        return commentService.GetAllAsync(page, pageSize, sortBy, asc);
    }
}
