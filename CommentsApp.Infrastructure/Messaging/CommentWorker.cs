using System.Text;
using System.Text.Json;
using CommentsApp.Application.DTOs;
using CommentsApp.Application.Interfaces;
using CommentsApp.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class CommentWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queue;

    public CommentWorker(
        RabbitMqConnectionFactory factory,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
        _queue = config["RabbitMq:Queue"]!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _queue,
            durable: true,
            exclusive: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                var data = JsonSerializer.Deserialize<CommentCreatedEvent>(body);
                
                using var scope = _scopeFactory.CreateScope();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var elastic = scope.ServiceProvider.GetRequiredService<IElasticService>();

                await cache.RemoveByPrefixAsync("comments_page_");

                if (data != null && data.Type == "CommentCreated")
                {
                    await elastic.IndexComment(new CommentIndex
                    {
                        Id = data.CommentId,
                        Text = data.Text,
                        UserName = data.UserName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker error: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(
            queue: _queue,
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}