using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using MicroFin.Customers.Api.Application.Dtos;
using MicroFin.Customers.Api.Application.Services;
using MicroFin.Customers.Api.Domain;
using MicroFin.Customers.Api.Infrastructure.Data;
using Xunit;

namespace MicroFin.Customers.Api.Tests.Unit.Application.Services;

/// <summary>
/// Testes unitários para o serviço de clientes.
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<ICpfValidationService> _cpfValidationServiceMock = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(
            _customerRepositoryMock.Object,
            _cpfValidationServiceMock.Object,
            _cache);
    }

    // ==================== CreateCustomerAsync ====================

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ReturnsCreatedResponse()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678901",
            ValorLimite = 1000.00m
        };

        _cpfValidationServiceMock.Setup(x => x.TryNormalizeAndValidate("12345678901", out It.Ref<string>.IsAny))
            .Returns((string cpf, out string normalized) => {
                normalized = "12345678901";
                return true;
            });
        _customerRepositoryMock.Setup(x => x.GetByCpfAsync("12345678901"))
            .ReturnsAsync((Customer?)null);

        var createdCustomer = new Customer("João da Silva", "12345678901", 1000.00m);
        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _service.CreateCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IdCliente.Should().Be(createdCustomer.Id);
        result.Status.Should().Be("OK");
    }

    [Fact]
    public async Task CreateCustomerAsync_WithDuplicateCpf_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678901",
            ValorLimite = 1000.00m
        };

        _cpfValidationServiceMock.Setup(x => x.TryNormalizeAndValidate("12345678901", out It.Ref<string>.IsAny))
            .Returns((string cpf, out string normalized) => {
                normalized = "12345678901";
                return true;
            });
        var existingCustomer = new Customer("João", "12345678901", 1000.00m);
        _customerRepositoryMock.Setup(x => x.GetByCpfAsync("12345678901"))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCustomerAsync(request));
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidCpf_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "123",
            ValorLimite = 1000.00m
        };

        _cpfValidationServiceMock.Setup(x => x.TryNormalizeAndValidate("123", out It.Ref<string>.IsAny))
            .Returns((string cpf, out string normalized) => {
                normalized = string.Empty;
                return false;
            });

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateCustomerAsync(request));
    }

    [Fact]
    public async Task CreateCustomerAsync_WithNegativeLimit_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678901",
            ValorLimite = -100.00m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateCustomerAsync(request));
    }

    [Fact]
    public async Task CreateCustomerAsync_CallsRepositoryCreateOnce()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678901",
            ValorLimite = 1000.00m
        };

        _cpfValidationServiceMock.Setup(x => x.TryNormalizeAndValidate("12345678901", out It.Ref<string>.IsAny))
            .Returns((string cpf, out string normalized) => {
                normalized = "12345678901";
                return true;
            });
        _customerRepositoryMock.Setup(x => x.GetByCpfAsync("12345678901"))
            .ReturnsAsync((Customer?)null);
        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(new Customer("João da Silva", "12345678901", 1000.00m));

        // Act
        await _service.CreateCustomerAsync(request);

        // Assert
        _customerRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_InvalidatesCache()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Nome = "João da Silva",
            Cpf = "12345678901",
            ValorLimite = 1000.00m
        };

        _cpfValidationServiceMock.Setup(x => x.TryNormalizeAndValidate("12345678901", out It.Ref<string>.IsAny))
            .Returns((string cpf, out string normalized) => {
                normalized = "12345678901";
                return true;
            });
        _customerRepositoryMock.Setup(x => x.GetByCpfAsync("12345678901"))
            .ReturnsAsync((Customer?)null);
        _customerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(new Customer("João da Silva", "12345678901", 1000.00m));

        // Pré-popular cache
        var cacheKey = "customers_list";
        _cache.Set(cacheKey, new GetCustomersResponse());

        // Act
        await _service.CreateCustomerAsync(request);

        // Assert
        _cache.Get(cacheKey).Should().BeNull();
    }

    // ==================== GetAllCustomersAsync ====================

    [Fact]
    public async Task GetAllCustomersAsync_WithNoCachedData_ReturnsFromRepository()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new("João", "12345678901", 1000.00m),
            new("Maria", "12345678902", 2000.00m)
        };

        _customerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("OK");
        result.Clientes.Should().HaveCount(2);
        result.Clientes.First().Nome.Should().Be("João");
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithCachedData_ReturnsFromCache()
    {
        // Arrange
        var cachedResponse = new GetCustomersResponse
        {
            Clientes = new List<CustomerListItem>
            {
                new() { IdCliente = Guid.NewGuid(), Nome = "Cached João", Cpf = "12345678901", ValorLimite = 1000.00m }
            },
            Status = "OK"
        };

        _cache.Set("customers_list", cachedResponse);

        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        result.Should().BeSameAs(cachedResponse);
        _customerRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllCustomersAsync_PopulatesCache()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new("João", "12345678901", 1000.00m)
        };

        _customerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        // Act
        await _service.GetAllCustomersAsync();

        // Assert
        var cachedValue = _cache.Get<GetCustomersResponse>("customers_list");
        cachedValue.Should().NotBeNull();
        cachedValue?.Clientes.Should().HaveCount(1);
    }

    // ==================== ProcessUpdateLimitMessageAsync ====================

    [Fact]
    public async Task ProcessUpdateLimitMessageAsync_WithUnprocessedMessageAndSufficientLimit_ReturnsTrue()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        _customerRepositoryMock.Setup(x => x.IsMessageProcessedAsync(messageId))
            .ReturnsAsync(false);
        _customerRepositoryMock.Setup(x => x.UpdateLimitAtomicAsync(customerId, amount))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessUpdateLimitMessageAsync(messageId, customerId, amount);

        // Assert
        result.Should().BeTrue();
        _customerRepositoryMock.Verify(x => x.RecordProcessedMessageAsync(messageId, customerId, amount), Times.Once);
    }

    [Fact]
    public async Task ProcessUpdateLimitMessageAsync_WithAlreadyProcessedMessage_ReturnsTrueAndDoesNotProcess()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        _customerRepositoryMock.Setup(x => x.IsMessageProcessedAsync(messageId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessUpdateLimitMessageAsync(messageId, customerId, amount);

        // Assert
        result.Should().BeTrue();
        _customerRepositoryMock.Verify(x => x.UpdateLimitAtomicAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        _customerRepositoryMock.Verify(x => x.RecordProcessedMessageAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessUpdateLimitMessageAsync_WithUnprocessedMessageAndInsufficientLimit_ReturnsFalse()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var amount = 1000.00m;

        _customerRepositoryMock.Setup(x => x.IsMessageProcessedAsync(messageId))
            .ReturnsAsync(false);
        _customerRepositoryMock.Setup(x => x.UpdateLimitAtomicAsync(customerId, amount))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ProcessUpdateLimitMessageAsync(messageId, customerId, amount);

        // Assert
        result.Should().BeFalse();
        _customerRepositoryMock.Verify(x => x.RecordProcessedMessageAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessUpdateLimitMessageAsync_WithUnprocessedMessageAndSufficientLimit_InvalidatesCache()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        _customerRepositoryMock.Setup(x => x.IsMessageProcessedAsync(messageId))
            .ReturnsAsync(false);
        _customerRepositoryMock.Setup(x => x.UpdateLimitAtomicAsync(customerId, amount))
            .ReturnsAsync(true);

        // Pré-popular cache
        _cache.Set("customers_list", new GetCustomersResponse());

        // Act
        await _service.ProcessUpdateLimitMessageAsync(messageId, customerId, amount);

        // Assert
        _cache.Get("customers_list").Should().BeNull();
    }

    [Fact]
    public async Task ProcessUpdateLimitMessageAsync_RecordsProcessedMessageOnSuccess()
    {
        // Arrange
        var messageId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid();
        var amount = 100.00m;

        _customerRepositoryMock.Setup(x => x.IsMessageProcessedAsync(messageId))
            .ReturnsAsync(false);
        _customerRepositoryMock.Setup(x => x.UpdateLimitAtomicAsync(customerId, amount))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessUpdateLimitMessageAsync(messageId, customerId, amount);

        // Assert
        _customerRepositoryMock.Verify(
            x => x.RecordProcessedMessageAsync(messageId, customerId, amount),
            Times.Once);
    }
}
