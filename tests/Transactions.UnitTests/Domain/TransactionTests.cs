using MicroFin.Transactions.Api.Domain;
using System;
using Xunit;

namespace Transactions.UnitTests.Domain;

public class TransactionTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesTransactionWithGuid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act
        var transaction = new Transaction(customerId, amount, "APROVADO");

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(customerId, transaction.IdCliente);
        Assert.Equal(amount, transaction.ValorSimulacao);
        Assert.Equal("APROVADO", transaction.Status);
        Assert.Equal(DateTime.UtcNow, transaction.CreatedAtUtc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ThrowsArgumentException()
    {
        // Arrange
        var amount = 100.00m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(Guid.Empty, amount, "APROVADO"));
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(customerId, 0, "APROVADO"));
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(customerId, -100, "APROVADO"));
    }

    [Fact]
    public void Constructor_WithNullStatus_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(customerId, amount, null!));
    }

    [Fact]
    public void Constructor_WithEmptyStatus_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(customerId, amount, string.Empty));
    }

    [Fact]
    public void Constructor_WithInvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Transaction(customerId, amount, "INVALIDO"));
    }

    [Fact]
    public void CreateApproved_CreatesTransactionWithApprovedStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act
        var transaction = Transaction.CreateApproved(customerId, amount);

        // Assert
        Assert.Equal("APROVADO", transaction.Status);
        Assert.Equal(customerId, transaction.IdCliente);
        Assert.Equal(amount, transaction.ValorSimulacao);
    }

    [Fact]
    public void CreateDenied_CreatesTransactionWithDeniedStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        // Act
        var transaction = Transaction.CreateDenied(customerId, amount);

        // Assert
        Assert.Equal("NEGADO", transaction.Status);
        Assert.Equal(customerId, transaction.IdCliente);
        Assert.Equal(amount, transaction.ValorSimulacao);
    }

    [Fact]
    public void CreateWithId_CreatesTransactionWithSpecificId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amount = 100.00m;
        var status = "APROVADO";
        var createdAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var transaction = Transaction.CreateWithId(id, customerId, amount, status, createdAt);

        // Assert
        Assert.Equal(id, transaction.Id);
        Assert.Equal(customerId, transaction.IdCliente);
        Assert.Equal(amount, transaction.ValorSimulacao);
        Assert.Equal(status, transaction.Status);
        Assert.Equal(createdAt, transaction.CreatedAtUtc);
    }

    [Fact]
    public void DefaultConstructor_CreatesTransactionWithDefaultValues()
    {
        // Act
        var transaction = new Transaction();

        // Assert
        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(Guid.Empty, transaction.IdCliente);
        Assert.Equal(0, transaction.ValorSimulacao);
        Assert.Equal(string.Empty, transaction.Status);
        Assert.Equal(DateTime.UtcNow, transaction.CreatedAtUtc, TimeSpan.FromSeconds(1));
    }
}
