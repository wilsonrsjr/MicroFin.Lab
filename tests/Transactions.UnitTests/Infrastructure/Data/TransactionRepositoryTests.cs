using MicroFin.Transactions.Api.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace Transactions.UnitTests.Infrastructure.Data;

public class TransactionRepositoryTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TransactionRepository(null!));
    }

    [Fact]
    public void Constructor_WithMissingConnectionString_ThrowsArgumentNullException()
    {
        // Arrange - Create a configuration without the connection string
        var configDict = new Dictionary<string, string?>
        {
            // No DefaultConnection
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TransactionRepository(configuration));
    }

    [Fact]
    public void Repository_ImplementsITransactionRepository()
    {
        // Verify that TransactionRepository implements ITransactionRepository
        Assert.True(typeof(ITransactionRepository).IsAssignableFrom(typeof(TransactionRepository)));
    }

    [Fact]
    public void Repository_CanBeInstantiatedWithValidConfiguration()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=127.0.0.1,1434;Database=MicroFin;"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act - This will create the repository but not actually connect
        // until a method is called
        var repository = new TransactionRepository(configuration);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Repository_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=127.0.0.1,1434;Database=MicroFin;"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act
        // We can't actually call methods without a real database connection
        // But we can verify the repository can be created
        var repository = new TransactionRepository(configuration);
        Assert.NotNull(repository);
    }
}
