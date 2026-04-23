using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
namespace CommentsApp.Infrastructure.Messaging
{
    public class RabbitMqConnectionFactory
    {
        private readonly IConfiguration _config;

        public RabbitMqConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IConnection> CreateAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMq:Host"],
                UserName = _config["RabbitMq:User"],
                Password = _config["RabbitMq:Pass"]
            };

            return await factory.CreateConnectionAsync();
        }
    }
}