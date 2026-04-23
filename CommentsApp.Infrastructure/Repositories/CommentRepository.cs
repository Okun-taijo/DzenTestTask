using CommentsApp.Application.Models;
using CommentsApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommentsApp.Infrastructure;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Comment>> GetAllFlatAsync()
    {
        return await _context.Comments
            .AsNoTracking()
            .Include(c=>c.Attachments)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetByIdsFlatAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new List<Comment>();
        }

        return await _context.Comments
            .AsNoTracking()
            .Include(c => c.Attachments)
            .Where(c => idList.Contains(c.Id))
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
    
}