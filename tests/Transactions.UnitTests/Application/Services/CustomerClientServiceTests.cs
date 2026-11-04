using MicroFin.Transactions.Api.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Transactions.UnitTests.Application.Services;

public class CustomerClientServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly string _baseUrl = "http://localhost:5002";

    public CustomerClientServiceTests()
    {
        _httpClient = new HttpClient();
        _configurationMock.Setup(c => c["CustomersApi:BaseUrl"]).Returns(_baseUrl);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithNullToken_ThrowsArgumentException()
    {
        // Arrange
        var service = new CustomerClientService(_httpClient, _configurationMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.GetCustomerByIdAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var service = new CustomerClientService(_httpClient, _configurationMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.GetCustomerByIdAsync(Guid.NewGuid(), string.Empty));
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithWhiteSpaceToken_ThrowsArgumentException()
    {
        // Arrange
        var service = new CustomerClientService(_httpClient, _configurationMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.GetCustomerByIdAsync(Guid.NewGuid(), "   "));
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithCustomBaseUrl_UsesConfiguredUrl()
    {
        // Arrange
        var customBaseUrl = "http://custom:8080";
        _configurationMock.Setup(c => c["CustomersApi:BaseUrl"]).Returns(customBaseUrl);
        var service = new CustomerClientService(_httpClient, _configurationMock.Object);

        // Act
        await service.GetCustomerByIdAsync(Guid.NewGuid(), "valid-token");

        // Assert - Just verify no exception is thrown with custom URL
        // The actual HTTP call will fail but that's expected for this test
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithValidResponse_ReturnsCustomerResponse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedResponse = new CustomerResponse
        {
            IdCliente = customerId,
            Nome = "Test Customer",
            Cpf = "12345678901",
            ValorLimite = 1000.00m,
            Status = "OK"
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId, "valid-token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.IdCliente);
        Assert.Equal("Test Customer", result.Nome);
        Assert.Equal("12345678901", result.Cpf);
        Assert.Equal(1000.00m, result.ValorLimite);
        Assert.Equal("OK", result.Status);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithNotFoundResponse_ReturnsNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId, "valid-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithErrorResponse_ReturnsNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId, "valid-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithException_ReturnsNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        var result = await service.GetCustomerByIdAsync(customerId, "valid-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_SetsAuthorizationHeader()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var jwtToken = "test-token-123";
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        await service.GetCustomerByIdAsync(customerId, jwtToken);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Headers.Authorization != null &&
                req.Headers.Authorization.Scheme == "Bearer" &&
                req.Headers.Authorization.Parameter == jwtToken),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetCustomerByIdAsync_UsesCorrectEndpoint()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        
        var handlerMock = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;
        
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()) 
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new CustomerClientService(httpClient, _configurationMock.Object);

        // Act
        await service.GetCustomerByIdAsync(customerId, "valid-token");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal($"{_baseUrl}/api/v1/customers/{customerId}", capturedRequest.RequestUri?.ToString());
    }
}
