using MediatR;
using ms.rabbitmq.Consumer;
using ms.rabbitmq.Events;
using ms.user.application.Commands;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ms.user.api.Consumers
{
    public class UsersConsumer(IServiceProvider serviceProvider, IConfiguration configuration) : IConsumer
    {
        private IConnection? _connection;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;



        public async Task Subscribe()
        {
            try
            {
                Console.WriteLine("Starting subscription to RabbitMQ...");

                var factory = new ConnectionFactory()
                {
                    HostName = _configuration.GetSection("Communications:EventBus:HostName").Value!,
                };

                _connection = await factory.CreateConnectionAsync();
                var channel = await _connection.CreateChannelAsync();
                var queue = typeof(AuthAccountCreatedEvent).Name;

                Console.WriteLine($"Declaring queue: {queue}");

                await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += ReceivedEvent;

                Console.WriteLine($"Subscribing to queue: {queue}");

                await channel.BasicConsumeAsync(queue: queue, autoAck: true, consumer: consumer);

                Console.WriteLine("Subscription to RabbitMQ success.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error to Subscribe to RabbitMq: {ex.Message}");
            }
        }

        public void Unsubscribe()
        {
            if (_connection != null) _connection.Dispose();
        }

        private async Task ReceivedEvent(object sender, BasicDeliverEventArgs ea)
        {
            Console.WriteLine("Event Received");
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var message = Encoding.UTF8.GetString(ea.Body.Span);
                var authAccountCreatedEvent = JsonSerializer.Deserialize<AuthAccountCreatedEvent>(message);
                var result = await mediator.Send(new CreateUserCommand(authAccountCreatedEvent.Id, authAccountCreatedEvent.UserName, authAccountCreatedEvent.Email));
            }
        }
    }
}
