using MicroFin.Customers.Api.Domain;
using MicroFin.Customers.Api.Application.Dtos;

namespace MicroFin.Customers.Api.Application.Services;

/// <summary>
/// Interface para serviço de negócio de clientes.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    /// <param name="request">Dados do cliente a ser criado.</param>
    /// <returns>Response com ID do cliente criado.</returns>
    /// <exception cref="ArgumentException">Se os dados forem inválidos.</exception>
    /// <exception cref="InvalidOperationException">Se o CPF já estiver cadastrado.</exception>
    Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);

    /// <summary>
    /// Obtém a lista de todos os clientes.
    /// </summary>
    /// <returns>Lista de clientes.</returns>
    Task<GetCustomersResponse> GetAllCustomersAsync();

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <returns>Response com o cliente ou null se não encontrado.</returns>
    Task<GetCustomerByIdResponse?> GetCustomerByIdAsync(Guid id);

    /// <summary>
    /// Processa uma mensagem de atualização de limite (RabbitMQ).
    /// </summary>
    /// <param name="messageId">ID único da mensagem.</param>
    /// <param name="customerId">ID do cliente.</param>
    /// <param name="amount">Valor a ser debitado.</param>
    /// <returns>True se a atualização foi bem-sucedida; false caso contrário.</returns>
    Task<bool> ProcessUpdateLimitMessageAsync(string messageId, Guid customerId, decimal amount);
}
