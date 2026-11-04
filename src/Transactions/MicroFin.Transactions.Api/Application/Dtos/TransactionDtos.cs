using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MicroFin.Transactions.Api.Application.Dtos;

/// <summary>
/// Request DTO para autorização de transação.
/// </summary>
public class AuthorizeRequest
{
    /// <summary>
    /// ID do cliente.
    /// </summary>
    [Required(ErrorMessage = "ID do cliente é obrigatório.")]
    [JsonPropertyName("idCliente")]
    public Guid IdCliente { get; set; }

    /// <summary>
    /// Valor da simulação.
    /// </summary>
    [Required(ErrorMessage = "Valor da simulação é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor da simulação deve ser maior que zero.")]
    [JsonPropertyName("valorSimulacao")]
    public decimal ValorSimulacao { get; set; }
}

/// <summary>
/// Response DTO para transação aprovada.
/// </summary>
public class AuthorizeResponse
{
    /// <summary>
    /// Status da autorização.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// ID único da transação (apenas quando aprovado).
    /// </summary>
    [JsonPropertyName("idTransacao")]
    public Guid? IdTransacao { get; set; }
}
