using MicroFin.Transactions.Api.Application.Dtos;
using MicroFin.Transactions.Api.Application.Services;
using MicroFin.Transactions.Api.Infrastructure.Data;
using MicroFin.Transactions.Api.Infrastructure.Messaging;
using Moq;
using System;
using Xunit;

namespace Transactions.UnitTests.Application.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
    private readonly Mock<ICustomerClientService> _customerClientMock = new();
    private readonly Mock<IRabbitMqPublisherService> _rabbitMqMock = new();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(
            _transactionRepositoryMock.Object,
            _customerClientMock.Object,
            _rabbitMqMock.Object);
    }

    [Fact]
    public async Task AuthorizeAsync_WithNullRequest_ReturnsDenied()
    {
        // Act
        var result = await _service.AuthorizeAsync(null!, "valid-token");

        // Assert
        Assert.Equal("NEGADO", result.Status);
        Assert.Null(result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_WithInvalidJwtToken_ReturnsDenied()
    {
        // Arrange
        var request = new AuthorizeRequest
        {
            IdCliente = Guid.NewGuid(),
            ValorSimulacao = 100.00m
        };

        // Act
        var result = await _service.AuthorizeAsync(request, null!);

        // Assert
        Assert.Equal("NEGADO", result.Status);
    }

    [Fact]
    public async Task AuthorizeAsync_CustomerNotFound_ReturnsDenied()
    {
        // Arrange
        var request = new AuthorizeRequest
        {
            IdCliente = Guid.NewGuid(),
            ValorSimulacao = 100.00m
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(request.IdCliente, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerResponse?)null);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        Assert.Equal("NEGADO", result.Status);
        Assert.Null(result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_InsufficientLimit_ReturnsDenied()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new AuthorizeRequest
        {
            IdCliente = customerId,
            ValorSimulacao = 1000.00m
        };

        var customer = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 500.00m,
            Status = "OK"
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        Assert.Equal("NEGADO", result.Status);
        Assert.Null(result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_ZeroAmount_ReturnsDenied()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new AuthorizeRequest
        {
            IdCliente = customerId,
            ValorSimulacao = 0.00m
        };

        var customer = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 500.00m,
            Status = "OK"
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        Assert.Equal("NEGADO", result.Status);
        Assert.Null(result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_NegativeAmount_ReturnsDenied()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new AuthorizeRequest
        {
            IdCliente = customerId,
            ValorSimulacao = -100.00m
        };

        var customer = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 500.00m,
            Status = "OK"
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        Assert.Equal("NEGADO", result.Status);
        Assert.Null(result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_ValidRequest_ReturnsApproved()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;
        var request = new AuthorizeRequest
        {
            IdCliente = customerId,
            ValorSimulacao = amount
        };

        var customer = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 500.00m,
            Status = "OK"
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _transactionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<MicroFin.Transactions.Api.Domain.Transaction>()))
            .ReturnsAsync((MicroFin.Transactions.Api.Domain.Transaction t) => t);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        Assert.Equal("APROVADO", result.Status);
        Assert.NotNull(result.IdTransacao);
        Assert.NotEqual(Guid.Empty, result.IdTransacao);
    }

    [Fact]
    public async Task AuthorizeAsync_ValidRequest_PublishesMessageToRabbitMQ()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var amount = 100.00m;
        var request = new AuthorizeRequest
        {
            IdCliente = customerId,
            ValorSimulacao = amount
        };

        var customer = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 500.00m,
            Status = "OK"
        };

        _customerClientMock.Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _transactionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<MicroFin.Transactions.Api.Domain.Transaction>()))
            .ReturnsAsync((MicroFin.Transactions.Api.Domain.Transaction t) => t);

        // Act
        var result = await _service.AuthorizeAsync(request, "valid-token");

        // Assert
        _rabbitMqMock.Verify(x => x.PublishAsync(
            "microfin.transactions",
            "transaction.authorized",
            "microfin.customers.update-limit",
            It.IsAny<object>()), Times.Once);
    }
}
