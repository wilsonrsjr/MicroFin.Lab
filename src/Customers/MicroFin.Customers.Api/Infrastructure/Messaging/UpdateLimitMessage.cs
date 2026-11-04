using System.Text.Json.Serialization;

namespace MicroFin.Customers.Api.Infrastructure.Messaging;

/// <summary>
/// DTO para mensagem de atualização de limite do cliente.
/// Enviada pela API Transactions via RabbitMQ.
/// </summary>
public class UpdateLimitMessage
{
    /// <summary>
    /// Identificador único da mensagem (usado para idempotência).
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// ID do cliente cujo limite será atualizado.
    /// </summary>
    [JsonPropertyName("customerId")]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Valor a ser debitado do limite do cliente.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
