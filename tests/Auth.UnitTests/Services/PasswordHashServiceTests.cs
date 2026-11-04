using Xunit;
using MicroFin.Auth.Api.Application.Services;

namespace MicroFin.Auth.Api.Tests.Unit.Services;

/// <summary>
/// Testes unitários para o serviço de hash de senha.
/// </summary>
public class PasswordHashServiceTests
{
    private readonly PasswordHashService _service = new();

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "ValidPassword123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash); // Hash não deve ser igual à senha
    }

    [Fact]
    public void HashPassword_WithSamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "ValidPassword123!";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Cada hash deve ser único (por causa do salt)
    }

    [Fact]
    public void HashPassword_WithNullPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.HashPassword(null!));
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.HashPassword(""));
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "ValidPassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var isValid = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "ValidPassword123!";
        var hash = _service.HashPassword(password);
        var wrongPassword = "WrongPassword123!";

        // Act
        var isValid = _service.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _service.HashPassword("ValidPassword123!");

        // Act
        var isValid = _service.VerifyPassword(null!, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ReturnsFalse()
    {
        // Act
        var isValid = _service.VerifyPassword("ValidPassword123!", null!);

        // Assert
        Assert.False(isValid);
    }
}
