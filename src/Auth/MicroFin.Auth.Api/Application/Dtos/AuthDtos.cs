namespace MicroFin.Auth.Api.Application.Dtos;

/// <summary>
/// Request para registro de novo usuário.
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// E-mail do usuário a ser registrado.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Senha em texto puro (deve ser validada antes de ser processada).
    /// </summary>
    public required string Password { get; init; }
}

/// <summary>
/// Response de sucesso para registro de usuário.
/// </summary>
public record RegisterResponse
{
    /// <summary>
    /// ID do usuário criado.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// E-mail do usuário registrado.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Mensagem de sucesso.
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Request para login de um usuário.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// E-mail do usuário.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Senha em texto puro.
    /// </summary>
    public required string Password { get; init; }
}

/// <summary>
/// Response de sucesso para login (contém JWT Bearer).
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// Tipo de token (sempre "Bearer" para JWT).
    /// </summary>
    public required string TokenType { get; init; }

    /// <summary>
    /// Token JWT para incluir no header Authorization.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Tempo de expiração do token em segundos.
    /// </summary>
    public required int ExpiresInSeconds { get; init; }
}
