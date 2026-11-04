using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace MicroFin.Auth.Api.Application.Services;

/// <summary>
/// Interface para serviço de geração e validação de JWT.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Gera um novo token JWT para um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="email">E-mail do usuário (será incluído como claim).</param>
    /// <returns>Token JWT em formato string.</returns>
    string GenerateToken(Guid userId, string email);

    /// <summary>
    /// Obtém o tempo de expiração configurado do token em segundos.
    /// </summary>
    int TokenExpirationSeconds { get; }
}

/// <summary>
/// Implementação de serviço JWT usando System.IdentityModel.Tokens.Jwt.
/// Gera tokens com assinatura, issuer, audience e expiração configuráveis.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _jwtKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationSeconds;

    public int TokenExpirationSeconds => _expirationSeconds;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Buscar configurações com fallback para valores padrão
        _jwtKey = _configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Jwt:Key não configurado. Use User Secrets ou variáveis de ambiente.");

        _issuer = _configuration["Jwt:Issuer"] ?? "MicroFin.Auth.Api";
        _audience = _configuration["Jwt:Audience"] ?? "MicroFin.Api";

        // Tentar ler ExpirationSeconds primeiro, depois ExpirationMinutes por compatibilidade
        if (!int.TryParse(_configuration["Jwt:ExpirationSeconds"], out var expirationSeconds))
        {
            if (int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var expirationMinutes))
            {
                expirationSeconds = expirationMinutes * 60;
            }
            else
            {
                expirationSeconds = 3600; // 1 hora padrão
            }
        }
        _expirationSeconds = expirationSeconds;

        // Validação básica
        if (_jwtKey.Length < 32)
        {
            throw new InvalidOperationException("Chave JWT deve ter pelo menos 32 caracteres para segurança.");
        }
    }

    public string GenerateToken(Guid userId, string email)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID não pode ser vazio.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-mail não pode ser nulo ou vazio.", nameof(email));
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expiration = now.AddSeconds(_expirationSeconds);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(token);
    }
}
