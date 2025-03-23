using Microsoft.AspNetCore.Mvc;
using QuickLogger.Domain.Dto;
using QuickLogger.Rabbitmq;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using System.Threading.Channels;
using QuickLogger.Application.Interfaces;
using System.Reflection;
using System.Linq.Expressions;

namespace QuickLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class LoggerController : ControllerBase
{
    private readonly IDatabaseHandlerFactory _databaseHandlerFactory;
    private readonly ILogger<LoggerController> _logger;
    private readonly RabbitMqConnection _rabbitMqConnection;
    private IConfiguration _config;
    public LoggerController(RabbitMqConnection rabbitMqConnection, IConfiguration config, IDatabaseHandlerFactory databaseHandlerFactory, ILogger<LoggerController> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _config = config;
        _databaseHandlerFactory = databaseHandlerFactory;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Log([FromBody] Log data)
    {
        var channel = _rabbitMqConnection.GetChannel();
        string queueName = _config["QueueName"]??"QuickLogger";

        await channel.ExchangeDeclareAsync("dead_letter_exchange", ExchangeType.Direct);
        await channel.QueueDeclareAsync("dead_letter_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync("dead_letter_queue", "dead_letter_exchange", "error");

        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" }
        };
        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);

        var jsonMessage = JsonSerializer.Serialize(data);
        byte[] body = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync("", queueName, body);

        return Ok();
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAll([FromBody] LogList data)
    {
        var dbhandler = await _databaseHandlerFactory.GetDatabaseHandlerByAppAsync(data.AppId);
        if (dbhandler == null) return BadRequest(new { error = "App not found" });

        var repo = await dbhandler.GetLogsRepositoryAsync();

        // Obtener la propiedad a ordenar de la entidad Log
        var propertyInfo = typeof(Log).GetProperty(data.OrderByProperty ?? "DateTime");
        if (propertyInfo == null)
        {
            return BadRequest(new { error = $"Property '{data.OrderByProperty}' not found in Log" });
        }

        // Usamos reflexión para construir la expresión lambda de ordenamiento dinámico
        var parameter = Expression.Parameter(typeof(Log), "log");
        var propertyAccess = Expression.Property(parameter, propertyInfo);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        // Llamar a GetSortedAsync dinámicamente
        var method = typeof(IRepository<Log, Guid>).GetMethod("GetSortedAsync")!
            .MakeGenericMethod(propertyInfo.PropertyType);

        var result = await (Task<IEnumerable<Log>>)method.Invoke(repo, new object[]
        {
            orderByExpression.Compile(),  // Func<Log, TKey>
            data.OrderDescending,         // Orden ascendente o descendente
            data.PageNumber,              // Número de página
            data.PageSize                 // Tamaño de página
        })!;

        return Ok(new { items= result});
    }
}
