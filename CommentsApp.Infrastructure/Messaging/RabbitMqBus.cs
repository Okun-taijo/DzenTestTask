using System.Text;
using CommentsApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
namespace CommentsApp.Infrastructure.Messaging
{
    public class RabbitMqBus : IMessageBus
    {
        private readonly RabbitMqConnectionFactory _factory;
        private readonly string _queue;

        public RabbitMqBus(RabbitMqConnectionFactory factory, IConfiguration config)
        {
            _factory = factory;
            _queue = config["RabbitMq:Queue"];
        }

        public async Task Publish(string message)
        {
            var connection = await _factory.CreateAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(_queue, durable: true, exclusive: false);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queue,
                body: body
            );
        }
    }
}