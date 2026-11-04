using Dapper;
using Microsoft.Data.SqlClient;
using MicroFin.Auth.Api.Domain;

namespace MicroFin.Auth.Api.Infrastructure.Data;

/// <summary>
/// Repositório para operações de persistência de usuários na base de dados.
/// Utiliza Dapper com SQL parametrizado para proteção contra SQL injection.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Cria um novo usuário na base de dados.
    /// </summary>
    /// <param name="user">Usuário a ser criado.</param>
    /// <param name="cancellationToken">Token de cancellation.</param>
    /// <returns>O usuário criado ou null se falhar.</returns>
    Task<User?> CreateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca um usuário por e-mail.
    /// </summary>
    /// <param name="email">E-mail do usuário a buscar.</param>
    /// <param name="cancellationToken">Token de cancellation.</param>
    /// <returns>O usuário encontrado ou null se não existir.</returns>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do repositório de usuários usando Dapper.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' não configurada.");
    }

    public async Task<User?> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO Users (Id, Email, PasswordHash, CreatedAtUtc)
            VALUES (@Id, @Email, @PasswordHash, @CreatedAtUtc);
        ";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                query,
                new
                {
                    user.Id,
                    user.Email,
                    user.PasswordHash,
                    user.CreatedAtUtc
                });

            return user;
        }
        catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
        {
            // E-mail duplicado
            return null;
        }
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT Id, Email, PasswordHash, CreatedAtUtc
            FROM Users
            WHERE Email = @Email;
        ";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var user = await connection.QueryFirstOrDefaultAsync<User>(
            query,
            new { Email = email });

        return user;
    }
}
