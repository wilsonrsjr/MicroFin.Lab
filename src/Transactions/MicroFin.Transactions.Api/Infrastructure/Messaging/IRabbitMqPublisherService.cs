namespace MicroFin.Transactions.Api.Infrastructure.Messaging;

/// <summary>
/// Interface para serviço de publicação de mensagens RabbitMQ.
/// </summary>
public interface IRabbitMqPublisherService
{
    /// <summary>
    /// Publica uma mensagem em um exchange com routing key.
    /// </summary>
    /// <param name="exchange">Nome do exchange.</param>
    /// <param name="routingKey">Routing key.</param>
    /// <param name="queue">Nome da fila (para garantir que existe).</param>
    /// <param name="message">Objeto a ser serializado para JSON.</param>
    Task PublishAsync(string exchange, string routingKey, string queue, object message);
}
