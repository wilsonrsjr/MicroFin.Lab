using System.Text;

namespace MicroFin.Customers.Api.Infrastructure.Messaging;

/// <summary>
/// Interface para serviço de consumo de mensagens RabbitMQ.
/// </summary>
public interface IRabbitMqConsumerService
{
    /// <summary>
    /// Inicia o consumo de mensagens de uma fila.
    /// </summary>
    /// <param name="queueName">Nome da fila.</param>
    /// <param name="onMessageReceived">Callback para processar a mensagem.</param>
    /// <param name="autoAck">Se true, confirma automaticamente o recebimento da mensagem.</param>
    void StartConsuming(string queueName, Func<string, Task<bool>> onMessageReceived, bool autoAck = false);

    /// <summary>
    /// Para o consumo de mensagens.
    /// </summary>
    void StopConsuming();
}

/// <summary>
/// Serviço de consumo de mensagens RabbitMQ.
/// Implementa consumo assíncrono com confirmação manual (ACK/NACK).
/// </summary>
public class RabbitMqConsumerService : IRabbitMqConsumerService, IDisposable
{
    private RabbitMQ.Client.IConnection? _connection;
    private RabbitMQ.Client.IModel? _channel;
    private string? _queueName;
    private Func<string, Task<bool>>? _onMessageReceived;
    private RabbitMQ.Client.Events.EventingBasicConsumer? _consumer;
    private bool _disposed = false;

    public RabbitMqConsumerService(IConfiguration configuration)
    {
        var host = configuration["RabbitMq:Host"] ?? "127.0.0.1";
        var port = configuration["RabbitMq:Port"] ?? "5672";

        var factory = new RabbitMQ.Client.ConnectionFactory
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
    /// Inicia o consumo de mensagens de uma fila.
    /// </summary>
    public void StartConsuming(string queueName, Func<string, Task<bool>> onMessageReceived, bool autoAck = false)
    {
        if (_channel == null) throw new InvalidOperationException("Channel not initialized");
        
        _queueName = queueName;
        _onMessageReceived = onMessageReceived;

        // Declarar a fila (idempotente)
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Limitar prefetch para 1 mensagem por vez (fair dispatch)
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(_channel);
        _consumer.Received += OnMessageReceivedAsync;

        _channel.BasicConsume(
            queue: queueName,
            autoAck: autoAck,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: _consumer);
    }

    private async void OnMessageReceivedAsync(object? sender, RabbitMQ.Client.Events.BasicDeliverEventArgs ea)
    {
        try
        {
            if (_onMessageReceived == null || _channel == null) return;

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Processar a mensagem
            var success = await _onMessageReceived(message);

            // Confirmar ou rejeitar a mensagem
            if (success)
            {
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            else
            {
                // Rejeitar sem requeue (para evitar loop infinito em caso de erro de negocio)
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        }
        catch (Exception)
        {
            // Rejeitar com requeue para erros transitórios
            if (_channel != null)
            {
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }

    /// <summary>
    /// Para o consumo de mensagens.
    /// </summary>
    public void StopConsuming()
    {
        if (_consumer != null)
        {
            _consumer.Received -= OnMessageReceivedAsync;
            _consumer = null;
        }

        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
        }

        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
        }
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
                StopConsuming();
                _channel?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    ~RabbitMqConsumerService()
    {
        Dispose(false);
    }
}
