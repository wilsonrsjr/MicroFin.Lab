using MicroFin.Customers.Api.Domain;

namespace MicroFin.Customers.Api.Infrastructure.Data;

/// <summary>
/// Interface para repositório de clientes.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Obtém um cliente pelo CPF.
    /// </summary>
    /// <param name="cpf">CPF do cliente (11 dígitos).</param>
    /// <returns>Cliente ou null se não encontrado.</returns>
    Task<Customer?> GetByCpfAsync(string cpf);

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <returns>Cliente ou null se não encontrado.</returns>
    Task<Customer?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém todos os clientes.
    /// </summary>
    /// <returns>Lista de clientes.</returns>
    Task<IEnumerable<Customer>> GetAllAsync();

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    /// <param name="customer">Cliente a ser criado.</param>
    /// <returns>Cliente criado com ID gerado.</returns>
    Task<Customer> CreateAsync(Customer customer);

    /// <summary>
    /// Atualiza o limite de um cliente de forma atômica.
    /// </summary>
    /// <param name="customerId">ID do cliente.</param>
    /// <param name="amount">Valor a ser debitado.</param>
    /// <returns>True se a atualização foi bem-sucedida; false caso contrário.</returns>
    Task<bool> UpdateLimitAtomicAsync(Guid customerId, decimal amount);

    /// <summary>
    /// Verifica se uma mensagem já foi processada.
    /// </summary>
    /// <param name="messageId">ID da mensagem.</param>
    /// <returns>True se já foi processada.</returns>
    Task<bool> IsMessageProcessedAsync(string messageId);

    /// <summary>
    /// Registra uma mensagem como processada.
    /// </summary>
    /// <param name="messageId">ID da mensagem.</param>
    /// <param name="customerId">ID do cliente.</param>
    /// <param name="amount">Valor debitado.</param>
    Task RecordProcessedMessageAsync(string messageId, Guid customerId, decimal amount);
}
