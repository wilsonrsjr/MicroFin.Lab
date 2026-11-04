using Dapper;
using Microsoft.Data.SqlClient;
using MicroFin.Customers.Api.Domain;

namespace MicroFin.Customers.Api.Infrastructure.Data;

/// <summary>
/// Implementação do repositório de clientes usando Dapper.
/// Utiliza SQL parametrizado para proteção contra SQL injection.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' não configurada.");
    }

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    /// <summary>
    /// Obtém um cliente pelo CPF.
    /// </summary>
    public async Task<Customer?> GetByCpfAsync(string cpf)
    {
        const string query = @"
            SELECT Id, Nome, CPF, ValorLimite, CreatedAtUtc, UpdatedAtUtc
            FROM Customers
            WHERE CPF = @Cpf;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var customer = await connection.QueryFirstOrDefaultAsync<Customer>(
            query,
            new { Cpf = cpf });

        return customer;
    }

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        const string query = @"
            SELECT Id, Nome, CPF, ValorLimite, CreatedAtUtc, UpdatedAtUtc
            FROM Customers
            WHERE Id = @Id;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var customer = await connection.QueryFirstOrDefaultAsync<Customer>(
            query,
            new { Id = id });

        return customer;
    }

    /// <summary>
    /// Obtém todos os clientes.
    /// </summary>
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        const string query = @"
            SELECT Id, Nome, CPF, ValorLimite, CreatedAtUtc, UpdatedAtUtc
            FROM Customers
            ORDER BY CreatedAtUtc;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var customers = await connection.QueryAsync<Customer>(query);
        return customers.ToList();
    }

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    public async Task<Customer> CreateAsync(Customer customer)
    {
        const string query = @"
            INSERT INTO Customers (Id, Nome, CPF, ValorLimite, CreatedAtUtc)
            VALUES (@Id, @Nome, @Cpf, @ValorLimite, @CreatedAtUtc);
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        try
        {
            await connection.ExecuteAsync(
                query,
                new
                {
                    customer.Id,
                    Nome = customer.Nome,
                    Cpf = customer.Cpf,
                    ValorLimite = customer.ValorLimite,
                    customer.CreatedAtUtc
                });

            return customer;
        }
        catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
        {
            throw new InvalidOperationException("CPF já cadastrado.", ex);
        }
    }

    /// <summary>
    /// Atualiza o limite de um cliente de forma atômica.
    /// </summary>
    public async Task<bool> UpdateLimitAtomicAsync(Guid customerId, decimal amount)
    {
        const string query = @"
            UPDATE Customers
            SET ValorLimite = ValorLimite - @Amount,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @CustomerId
            AND ValorLimite >= @Amount;

            SELECT @@ROWCOUNT;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            query,
            new
            {
                CustomerId = customerId,
                Amount = amount,
                UpdatedAtUtc = DateTime.UtcNow
            });

        return rowsAffected == 1;
    }

    /// <summary>
    /// Verifica se uma mensagem já foi processada.
    /// </summary>
    public async Task<bool> IsMessageProcessedAsync(string messageId)
    {
        const string query = @"
            SELECT 1 FROM ProcessedMessages WHERE MessageId = @MessageId;
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(query, new { MessageId = messageId });
        return exists;
    }

    /// <summary>
    /// Registra uma mensagem como processada.
    /// </summary>
    public async Task RecordProcessedMessageAsync(string messageId, Guid customerId, decimal amount)
    {
        const string query = @"
            INSERT INTO ProcessedMessages (MessageId, CustomerId, Amount)
            VALUES (@MessageId, @CustomerId, @Amount);
        ";

        using var connection = CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            query,
            new
            {
                MessageId = messageId,
                CustomerId = customerId,
                Amount = amount
            });
    }
}
