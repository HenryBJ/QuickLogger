using RabbitMQ.Client;

namespace QuickLogger.Rabbitmq;

public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqConnection(IConfiguration configuration)
    {
        ConnectionFactory factory = new ConnectionFactory();
        factory.Uri = new Uri(configuration["rabbitmq"]!);

        IConnection conn = factory.CreateConnectionAsync().Result;
        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;
    }

    public IChannel GetChannel() => _channel;

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
