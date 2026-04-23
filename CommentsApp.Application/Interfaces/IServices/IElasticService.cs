namespace CommentsApp.Application.Interfaces
{
    public interface IElasticService
    {
        Task InitAsync();
        Task IndexComment(CommentIndex comment);
        Task<List<CommentSearchResult>> Search(string query);
    }
}