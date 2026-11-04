using BCrypt.Net;

namespace MicroFin.Auth.Api.Application.Services;

/// <summary>
/// Interface para serviço de hash seguro de senha.
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Gera um hash bcrypt seguro para uma senha em texto puro.
    /// </summary>
    /// <param name="password">Senha em texto puro.</param>
    /// <returns>Hash bcrypt da senha.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifica se uma senha em texto puro corresponde ao hash armazenado.
    /// </summary>
    /// <param name="password">Senha em texto puro a ser verificada.</param>
    /// <param name="hash">Hash bcrypt armazenado.</param>
    /// <returns>True se a senha corresponde ao hash; false caso contrário.</returns>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Implementação de serviço de hash de senha usando BCrypt.
/// BCrypt inclui automaticamente salt e é resistente a ataques de força bruta.
/// </summary>
public class PasswordHashService : IPasswordHashService
{
    /// <summary>
    /// Fator de trabalho do bcrypt (10 é padrão e seguro, quanto maior mais lento mas mais seguro).
    /// </summary>
    private const int WorkFactor = 10;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Senha não pode ser nula ou vazia.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(WorkFactor));
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch 
        {
            // Se o hash for inválido, bcrypt pode lançar exceção
            return false;
        }
    }
}
