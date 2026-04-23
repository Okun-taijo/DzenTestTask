namespace CommentsApp.Application.Interfaces{
    public interface IMessageBus
    {
        Task Publish(string message);
    }
}