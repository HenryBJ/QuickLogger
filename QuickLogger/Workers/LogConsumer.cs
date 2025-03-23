using QuickLogger.Rabbitmq;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using QuickLogger.Domain.Dto;
using System.Threading.Channels;
using QuickLogger.Infrastructure.Common;
using QuickLogger.Application.Interfaces;
namespace QuickLogger.Workers;

public class LogConsumer : BackgroundService
{
    private readonly LogRouter _logRouter;
    private readonly RabbitMqConnection _rabbitMqConnection;
    private IChannel _channel;
    private IConfiguration _config;

    public LogConsumer(RabbitMqConnection rabbitMqConnection, IConfiguration config, LogRouter logRouter)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _channel = _rabbitMqConnection.GetChannel();
        _config = config;
        _logRouter = logRouter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int retryLimit = int.TryParse(_config["RetryLimit"], out int result) ? result : 3;
        
        string queueName = _config["QueueName"] ?? "QuickLogger";

        await _channel.ExchangeDeclareAsync("dead_letter_exchange", ExchangeType.Direct);
        await _channel.QueueDeclareAsync("dead_letter_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        await _channel.QueueBindAsync("dead_letter_queue", "dead_letter_exchange", "error");

        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" }
        };
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            
            try
            {
                var log = JsonSerializer.Deserialize<Log>(jsonMessage);
                await _logRouter.RouteLogAsync(log!);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };
        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
}
