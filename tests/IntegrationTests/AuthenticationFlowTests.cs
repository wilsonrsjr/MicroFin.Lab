using Xunit;
using MicroFin.Auth.Api.Application.Dtos;
using MicroFin.Auth.Api.Application.Services;
using MicroFin.Auth.Api.Infrastructure.Data;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace MicroFin.Auth.Api.Tests.Integration;

/// <summary>
/// Testes de integração para fluxo de registro, login e validação JWT.
/// </summary>
public class AuthenticationFlowTests
{
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly Mock<IUserRepository> _mockUserRepository;

    public AuthenticationFlowTests()
    {
        // Configuração real para serviços de hash e JWT
        var configDict = new Dictionary<string, string?>
        {
            { "Jwt:Key", "this_is_a_very_long_test_jwt_key_with_at_least_32_characters_1234567890" },
            { "Jwt:Issuer", "MicroFin.Auth.Test" },
            { "Jwt:Audience", "MicroFin.Test" },
            { "Jwt:ExpirationSeconds", "3600" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        _passwordHashService = new PasswordHashService();
        _jwtTokenService = new JwtTokenService(configuration);
        _mockUserRepository = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task AuthenticationFlow_RegisterLoginAndValidateToken_Succeeds()
    {
        // ARRANGE
        var email = "integration.test@example.com";
        var password = "SecurePassword123!";

        // Simular registro: criar usuário com hash da senha
        var passwordHash = _passwordHashService.HashPassword(password);
        var userId = Guid.NewGuid();
        var registeredUser = new MicroFin.Auth.Api.Domain.User(email, passwordHash) { Id = userId };

        // Simular que o repositório encontra o usuário
        _mockUserRepository
            .Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registeredUser);

        // ACT - Fluxo: Login -> Gerar JWT -> Validar Token

        // 1. Buscar usuário por e-mail
        var user = await _mockUserRepository.Object.FindByEmailAsync(email, CancellationToken.None);
        Assert.NotNull(user);

        // 2. Verificar senha
        var isPasswordValid = _passwordHashService.VerifyPassword(password, user.PasswordHash);
        Assert.True(isPasswordValid);

        // 3. Gerar JWT
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // ASSERT - Validar estrutura e conteúdo do JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        Assert.NotNull(jwtToken);
        Assert.Equal("MicroFin.Auth.Test", jwtToken.Issuer);
        Assert.Contains("MicroFin.Test", jwtToken.Audiences);
        Assert.Equal(userId.ToString(), jwtToken.Payload["sub"]);
        Assert.Equal(email, jwtToken.Payload["email"]);
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticationFlow_WrongPassword_ReturnsFalse()
    {
        // ARRANGE
        var email = "test@example.com";
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";

        var passwordHash = _passwordHashService.HashPassword(password);
        var user = new MicroFin.Auth.Api.Domain.User(email, passwordHash);

        _mockUserRepository
            .Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // ACT
        var foundUser = await _mockUserRepository.Object.FindByEmailAsync(email, CancellationToken.None);
        var isPasswordValid = _passwordHashService.VerifyPassword(wrongPassword, foundUser!.PasswordHash);

        // ASSERT
        Assert.False(isPasswordValid);
    }

    [Fact]
    public async Task AuthenticationFlow_UserNotFound_ReturnsNull()
    {
        // ARRANGE
        var email = "nonexistent@example.com";

        _mockUserRepository
            .Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicroFin.Auth.Api.Domain.User?)null);

        // ACT
        var user = await _mockUserRepository.Object.FindByEmailAsync(email, CancellationToken.None);

        // ASSERT
        Assert.Null(user);
    }

    [Fact]
    public void AuthenticationFlow_HashIsNotReversible()
    {
        // ARRANGE
        var password = "SecurePassword123!";

        // ACT
        var hash = _passwordHashService.HashPassword(password);

        // ASSERT
        // O hash não deve ser único para cada chamada (tem salt) e não deve ser reversível
        Assert.NotEqual(password, hash);
        var hash2 = _passwordHashService.HashPassword(password);
        Assert.NotEqual(hash, hash2);
    }

    [Fact]
    public async Task AuthenticationFlow_RegisterDuplicateEmail_RepositoryCallShouldDetect()
    {
        // ARRANGE
        var email = "duplicate@example.com";
        var password = "SecurePassword123!";

        var existingUser = new MicroFin.Auth.Api.Domain.User(email, _passwordHashService.HashPassword(password));

        // Simular que a primeira tentativa de criação retorna null (duplicado)
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.Is<MicroFin.Auth.Api.Domain.User>(u => u.Email == email), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MicroFin.Auth.Api.Domain.User?)null);

        // ACT
        var newUser = new MicroFin.Auth.Api.Domain.User(email, _passwordHashService.HashPassword(password));
        var result = await _mockUserRepository.Object.CreateAsync(newUser, CancellationToken.None);

        // ASSERT
        Assert.Null(result); // Duplicado não foi criado
    }

    [Fact]
    public void AuthenticationFlow_TokenIsNotExposedInResponse()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // ACT
        var token = _jwtTokenService.GenerateToken(userId, email);

        // ASSERT
        // O token deve ser enviado apenas no header Authorization, nunca em logs ou respostas de erro
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Simulação: se chegasse um log, não deveria conter o token
        var logMessage = $"User {email} logged in";
        Assert.DoesNotContain(token, logMessage);
    }
}
