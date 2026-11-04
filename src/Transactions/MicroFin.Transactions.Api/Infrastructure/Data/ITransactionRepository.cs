using MicroFin.Transactions.Api.Domain;

namespace MicroFin.Transactions.Api.Infrastructure.Data;

/// <summary>
/// Interface para repositório de transações.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Obtém uma transação pelo ID.
    /// </summary>
    /// <param name="id">ID da transação.</param>
    /// <returns>Transação ou null se não encontrado.</returns>
    Task<Transaction?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém transações por cliente.
    /// </summary>
    /// <param name="customerId">ID do cliente.</param>
    /// <returns>Lista de transações do cliente.</returns>
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    /// <param name="transaction">Transação a ser criada.</param>
    /// <returns>Transação criada.</returns>
    Task<Transaction> CreateAsync(Transaction transaction);

    /// <summary>
    /// Verifica se uma transação já existe.
    /// </summary>
    /// <param name="id">ID da transação.</param>
    /// <returns>True se existir.</returns>
    Task<bool> ExistsAsync(Guid id);
}
