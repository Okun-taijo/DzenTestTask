using CommentsApp.Application.Models;

namespace CommentsApp.Application.Interfaces;

public interface ICommentRepository
{
    Task AddAsync(Comment comment);
    Task<List<Comment>> GetAllFlatAsync();
    Task<List<Comment>> GetByIdsFlatAsync(IEnumerable<int> ids);
    Task SaveChangesAsync();
}