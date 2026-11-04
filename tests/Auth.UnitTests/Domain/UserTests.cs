using Xunit;
using MicroFin.Auth.Api.Domain;

namespace MicroFin.Auth.Api.Tests.Unit.Domain;

/// <summary>
/// Testes unitários para a entidade User.
/// </summary>
public class UserTests
{
    [Fact]
    public void UserConstructor_WithValidEmailAndHash_CreatesUserWithNewId()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "$2a$10$..."; // Simula um hash bcrypt

        // Act
        var user = new User(email, passwordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.NotEqual(DateTime.MinValue, user.CreatedAtUtc);
    }

    [Fact]
    public void UserConstructor_CreatesUserWithCurrentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var email = "test@example.com";
        var passwordHash = "$2a$10$...";

        // Act
        var user = new User(email, passwordHash);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(user.CreatedAtUtc >= before);
        Assert.True(user.CreatedAtUtc <= after);
    }

    [Fact]
    public void UserConstructor_EachCallCreatesUniqueId()
    {
        // Arrange & Act
        var user1 = new User("user1@example.com", "hash1");
        var user2 = new User("user2@example.com", "hash2");

        // Assert
        Assert.NotEqual(user1.Id, user2.Id);
    }

    [Fact]
    public void EmptyConstructor_CreatesUserWithEmptyValuesForDapper()
    {
        // Act
        var user = new User();

        // Assert
        Assert.Equal(Guid.Empty, user.Id);
        Assert.Empty(user.Email);
        Assert.Empty(user.PasswordHash);
    }
}
