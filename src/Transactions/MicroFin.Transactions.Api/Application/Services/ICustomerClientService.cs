using MicroFin.Transactions.Api.Application.Dtos;

namespace MicroFin.Transactions.Api.Application.Services;

/// <summary>
/// Interface para cliente HTTP da API de Customers.
/// </summary>
public interface ICustomerClientService
{
    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    /// <param name="customerId">ID do cliente.</param>
    /// <param name="jwtToken">Token JWT para autenticação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Response com o cliente ou null se não encontrado.</returns>
    Task<CustomerResponse?> GetCustomerByIdAsync(Guid customerId, string jwtToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO para resposta do cliente (usado pelo cliente HTTP).
/// </summary>
public class CustomerResponse
{
    public Guid IdCliente { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public decimal ValorLimite { get; set; }
    public string Status { get; set; } = string.Empty;
}
