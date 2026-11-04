namespace MicroFin.Auth.Api.Domain;

/// <summary>
/// Entidade de domínio que representa um usuário autenticado.
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único do usuário.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Endereço de e-mail do usuário (único no sistema).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash bcrypt da senha. Nunca exposto externamente.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do usuário em UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Construtor para criação de novo usuário.
    /// </summary>
    public User(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail não pode ser nulo ou vazio.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Hash de senha não pode ser nulo ou vazio.", nameof(passwordHash));

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Construtor sem parâmetros para desserialização (ex: Dapper).
    /// </summary>
#pragma warning disable CS8618
    public User()
    {
    }
#pragma warning restore CS8618
}


