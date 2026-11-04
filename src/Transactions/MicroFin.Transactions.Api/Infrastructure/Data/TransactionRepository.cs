using Dapper;
using Microsoft.Data.SqlClient;
using MicroFin.Transactions.Api.Domain;

namespace MicroFin.Transactions.Api.Infrastructure.Data;

/// <summary>
/// Implementação do repositório de transações usando Dapper.
/// Utiliza SQL parametrizado para proteção contra SQL injection.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' não configurada.");
    }

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    /// <summary>
    /// Obtém uma transação pelo ID.
    /// </summary>
    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        const string query = @"
            SELECT Id, IdCliente, ValorSimulacao, Status, CreatedAtUtc
            FROM Transactions
            WHERE Id = @Id;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var transaction = await connection.QueryFirstOrDefaultAsync<Transaction>(
            query,
            new { Id = id });

        return transaction;
    }

    /// <summary>
    /// Obtém transações por cliente.
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(Guid customerId)
    {
        const string query = @"
            SELECT Id, IdCliente, ValorSimulacao, Status, CreatedAtUtc
            FROM Transactions
            WHERE IdCliente = @CustomerId
            ORDER BY CreatedAtUtc DESC;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var transactions = await connection.QueryAsync<Transaction>(
            query,
            new { CustomerId = customerId });

        return transactions.ToList();
    }

    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        const string query = @"
            INSERT INTO Transactions (Id, IdCliente, ValorSimulacao, Status, CreatedAtUtc)
            VALUES (@Id, @IdCliente, @ValorSimulacao, @Status, @CreatedAtUtc);
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            query,
            new
            {
                transaction.Id,
                transaction.IdCliente,
                transaction.ValorSimulacao,
                transaction.Status,
                transaction.CreatedAtUtc
            });

        return transaction;
    }

    /// <summary>
    /// Verifica se uma transação já existe.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        const string query = @"
            SELECT 1 FROM Transactions WHERE Id = @Id;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        return exists;
    }
}
