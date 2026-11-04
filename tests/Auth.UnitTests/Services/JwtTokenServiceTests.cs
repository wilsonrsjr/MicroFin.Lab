using Xunit;
using Microsoft.Extensions.Configuration;
using MicroFin.Auth.Api.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MicroFin.Auth.Api.Tests.Unit.Services;

/// <summary>
/// Testes unitários para o serviço de geração JWT.
/// </summary>
public class JwtTokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly string _testJwtKey;

    public JwtTokenServiceTests()
    {
        _testJwtKey = "this_is_a_very_long_test_jwt_key_with_at_least_32_characters_1234567890";

        var configDictionary = new Dictionary<string, string?>
        {
            { "Jwt:Key", _testJwtKey },
            { "Jwt:Issuer", "MicroFin.Auth.Test" },
            { "Jwt:Audience", "MicroFin.Test" },
            { "Jwt:ExpirationSeconds", "3600" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDictionary)
            .Build();
    }

    [Fact]
    public void GenerateToken_WithValidUserId_ReturnsValidJwt()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var token = service.GenerateToken(userId, email);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validar estrutura do JWT (3 partes separadas por ponto)
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_WithValidToken_ContainsCorrectClaims()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var token = service.GenerateToken(userId, email);

        // Assert - Decodificar e validar claims
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jwtToken);
        Assert.Equal("MicroFin.Auth.Test", jwtToken.Issuer);
        Assert.Contains("MicroFin.Test", jwtToken.Audiences);
        Assert.NotNull(jwtToken.Payload["sub"]);
        Assert.Equal(userId.ToString(), jwtToken.Payload["sub"]);
        Assert.Equal(email, jwtToken.Payload["email"]);
    }

    [Fact]
    public void GenerateToken_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            service.GenerateToken(Guid.Empty, "test@example.com"));
    }

    [Fact]
    public void GenerateToken_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            service.GenerateToken(userId, null!));
    }

    [Fact]
    public void GenerateToken_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            service.GenerateToken(userId, ""));
    }

    [Fact]
    public void GenerateToken_ValidToken_ExpiresAtCorrectTime()
    {
        // Arrange
        var service = new JwtTokenService(_configuration);
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateToken(userId, email);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jwtToken!.ValidTo);

        // Validar que expiração é aproximadamente 3600 segundos no futuro
        var expectedExpiration = beforeGeneration.AddSeconds(3600);
        var actualExpiration = jwtToken.ValidTo;

        // Margem de 5 segundos para considerar tempo de execução
        Assert.True(actualExpiration >= expectedExpiration.AddSeconds(-5));
        Assert.True(actualExpiration <= expectedExpiration.AddSeconds(5));
    }

    [Fact]
    public void Constructor_WithMissingJwtKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var configDictionary = new Dictionary<string, string?>
        {
            { "Jwt:Issuer", "MicroFin.Auth.Test" }
            // Note: Jwt:Key está faltando
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDictionary)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new JwtTokenService(configuration));
    }

    [Fact]
    public void Constructor_WithShortJwtKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var configDictionary = new Dictionary<string, string?>
        {
            { "Jwt:Key", "short_key" }, // Menos de 32 caracteres
            { "Jwt:Issuer", "MicroFin.Auth.Test" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDictionary)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new JwtTokenService(configuration));
    }

    [Fact]
    public void TokenExpirationSeconds_ReadFromConfiguration()
    {
        // Arrange & Act
        var service = new JwtTokenService(_configuration);

        // Assert
        Assert.Equal(3600, service.TokenExpirationSeconds);
    }
}
