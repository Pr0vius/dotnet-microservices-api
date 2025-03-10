using Microsoft.Extensions.Configuration;
using ms.rabbitmq.Events;
using RabbitMQ.Client;
using System.Text;

namespace ms.rabbitmq.Producers
{
    public class EventProducer : IProducer
    {
        private readonly IConfiguration _configuration;
        public EventProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Produce(RabbitMqEvent rabbitMqEvent)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration.GetSection("Communications:EventBus:HostName").Value!,
                };
                using (var connection = await factory.CreateConnectionAsync())
                using (var channel = await connection.CreateChannelAsync())
                {
                    var queue = rabbitMqEvent.GetType().Name;
                    await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                    var body = Encoding.UTF8.GetBytes(rabbitMqEvent.Serialize());

                    Console.WriteLine($"Producing event to queue: {queue}");
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: queue,
                        body: body);
                };
            }catch(Exception ex)
            {
                Console.WriteLine($"Cant Produce the event: {ex.Message}");
            }
        }
    }
}
