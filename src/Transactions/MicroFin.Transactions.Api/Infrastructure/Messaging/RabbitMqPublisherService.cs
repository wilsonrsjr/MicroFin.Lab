using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MicroFin.Transactions.Api.Infrastructure.Messaging;

/// <summary>
/// Serviço de publicação de mensagens RabbitMQ.
/// </summary>
public class RabbitMqPublisherService : IRabbitMqPublisherService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed;

    public RabbitMqPublisherService(IConfiguration configuration)
    {
        var host = configuration["RabbitMq:Host"] ?? "127.0.0.1";
        var port = configuration["RabbitMq:Port"] ?? "5672";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = int.Parse(port),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    /// <summary>
    /// Publica uma mensagem em um exchange com routing key.
    /// </summary>
    public Task PublishAsync(string exchange, string routingKey, string queue, object message)
    {
        // Declarar exchange
        _channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);

        // Declarar fila (idempotente)
        _channel.QueueDeclare(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Bind queue to exchange
        _channel.QueueBind(queue, exchange, routingKey);

        // Serializar mensagem para JSON
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // Criar properties
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.DeliveryMode = 2; // Persistent

        // Publicar mensagem
        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_channel != null && _channel.IsOpen)
                {
                    _channel.Close();
                }
                if (_connection != null && _connection.IsOpen)
                {
                    _connection.Close();
                }
                _channel?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    ~RabbitMqPublisherService()
    {
        Dispose(false);
    }
}
