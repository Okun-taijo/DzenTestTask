namespace CommentsApp.Application.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task NewComment(object dto);
    }
}