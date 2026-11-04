using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MicroFin.Customers.Api.Application.Dtos;

/// <summary>
/// Request DTO para criação de cliente.
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres.")]
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente (11 dígitos numéricos).
    /// </summary>
    [Required(ErrorMessage = "CPF é obrigatório.")]
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>
    /// Valor do limite disponível para o cliente.
    /// </summary>
    [Required(ErrorMessage = "Valor do limite é obrigatório.")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor do limite não pode ser negativo.")]
    [JsonPropertyName("valorLimite")]
    public decimal ValorLimite { get; set; }
}

/// <summary>
/// Response DTO para criação de cliente (sucesso).
/// </summary>
public class CreateCustomerResponse
{
    /// <summary>
    /// Identificador único do cliente criado.
    /// </summary>
    [JsonPropertyName("idCliente")]
    public Guid IdCliente { get; set; }

    /// <summary>
    /// Status da operação.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "OK";
}

/// <summary>
/// Response DTO para erro na criação de cliente.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Status da operação.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "ERRO";

    /// <summary>
    /// Detalhe do erro.
    /// </summary>
    [JsonPropertyName("detalheErro")]
    public string DetalheErro { get; set; } = string.Empty;
}

/// <summary>
/// DTO para listagem de clientes.
/// </summary>
public class CustomerListItem
{
    /// <summary>
    /// Identificador único do cliente.
    /// </summary>
    [JsonPropertyName("idCliente")]
    public Guid IdCliente { get; set; }

    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente (apenas os 11 dígitos).
    /// </summary>
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>
    /// Valor do limite disponível.
    /// </summary>
    [JsonPropertyName("valorLimite")]
    public decimal ValorLimite { get; set; }
}

/// <summary>
/// Response DTO para listagem de clientes.
/// </summary>
public class GetCustomersResponse
{
    /// <summary>
    /// Lista de clientes.
    /// </summary>
    [JsonPropertyName("clientes")]
    public IEnumerable<CustomerListItem> Clientes { get; set; } = new List<CustomerListItem>();

    /// <summary>
    /// Status da operação.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "OK";
}

/// <summary>
/// Response DTO para obtenção de cliente por ID.
/// </summary>
public class GetCustomerByIdResponse
{
    /// <summary>
    /// Identificador único do cliente.
    /// </summary>
    [JsonPropertyName("idCliente")]
    public Guid IdCliente { get; set; }

    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente (apenas os 11 dígitos).
    /// </summary>
    [JsonPropertyName("cpf")]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>
    /// Valor do limite disponível.
    /// </summary>
    [JsonPropertyName("valorLimite")]
    public decimal ValorLimite { get; set; }

    /// <summary>
    /// Status da operação.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "OK";
}
