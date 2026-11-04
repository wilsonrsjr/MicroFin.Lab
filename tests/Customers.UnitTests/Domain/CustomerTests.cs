using FluentAssertions;
using MicroFin.Customers.Api.Domain;
using Xunit;

namespace MicroFin.Customers.Api.Tests.Unit.Domain;

/// <summary>
/// Testes unitários para a entidade Customer.
/// </summary>
public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCustomerWithCorrectProperties()
    {
        // Arrange
        var nome = "João da Silva";
        var cpf = "12345678901";
        var valorLimite = 1000.00m;

        // Act
        var customer = new Customer(nome, cpf, valorLimite);

        // Assert
        customer.Nome.Should().Be(nome);
        customer.Cpf.Should().Be(cpf);
        customer.ValorLimite.Should().Be(valorLimite);
        customer.Id.Should().NotBe(Guid.Empty);
        customer.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFormattedCpf_StoresOnlyDigits()
    {
        // Arrange
        var cpfFormatted = "123.456.789-01";

        // Act
        var customer = new Customer("Teste", cpfFormatted, 100.00m);

        // Assert
        customer.Cpf.Should().Be("12345678901");
    }

    [Fact]
    public void Constructor_WithCpfWithExtraCharacters_StoresOnlyDigits()
    {
        // Arrange
        var cpfWithExtra = "123-456.789-01 ";

        // Act
        var customer = new Customer("Teste", cpfWithExtra, 100.00m);

        // Assert
        customer.Cpf.Should().Be("12345678901");
    }

    [Fact]
    public void Constructor_WithNullNome_ThrowsArgumentException()
    {
        // Arrange
        string nullNome = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer(nullNome, "12345678901", 100.00m));
        exception.Message.Should().Contain("Nome");
    }

    [Fact]
    public void Constructor_WithEmptyNome_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer(string.Empty, "12345678901", 100.00m));
        exception.Message.Should().Contain("Nome");
    }

    [Fact]
    public void Constructor_WithWhiteSpaceNome_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("   ", "12345678901", 100.00m));
        exception.Message.Should().Contain("Nome");
    }

    [Fact]
    public void Constructor_WithNomeTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longNome = new string('A', 256);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer(longNome, "12345678901", 100.00m));
        exception.Message.Should().Contain("255");
    }

    [Fact]
    public void Constructor_WithNullCpf_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("Teste", null!, 100.00m));
        exception.Message.Should().Contain("CPF");
    }

    [Fact]
    public void Constructor_WithEmptyCpf_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("Teste", string.Empty, 100.00m));
        exception.Message.Should().Contain("CPF");
    }

    [Fact]
    public void Constructor_WithCpfTooShort_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("Teste", "12345", 100.00m));
        exception.Message.Should().Contain("11 dígitos");
    }

    [Fact]
    public void Constructor_WithCpfTooLong_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("Teste", "123456789012", 100.00m));
        exception.Message.Should().Contain("11 dígitos");
    }

    [Fact]
    public void Constructor_WithNegativeValorLimite_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Customer("Teste", "12345678901", -100.00m));
        exception.Message.Should().Contain("negativo");
    }

    [Fact]
    public void DebitLimit_WithValidAmount_ReducesLimitAndSetsUpdatedAt()
    {
        // Arrange
        var customer = new Customer("Teste", "12345678901", 1000.00m);
        var initialLimit = customer.ValorLimite;

        // Act
        customer.DebitLimit(200.00m);

        // Assert
        customer.ValorLimite.Should().Be(initialLimit - 200.00m);
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DebitLimit_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var customer = new Customer("Teste", "12345678901", 1000.00m);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => customer.DebitLimit(-100.00m));
    }

    [Fact]
    public void DebitLimit_WithAmountGreaterThanLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var customer = new Customer("Teste", "12345678901", 100.00m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => customer.DebitLimit(150.00m));
    }

    [Fact]
    public void DebitLimit_WithExactAmount_SetsLimitToZero()
    {
        // Arrange
        var customer = new Customer("Teste", "12345678901", 100.00m);

        // Act
        customer.DebitLimit(100.00m);

        // Assert
        customer.ValorLimite.Should().Be(0);
    }

    [Fact]
    public void CreateWithId_CreatesCustomerWithSpecifiedId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var customer = Customer.CreateWithId(id, "Teste", "12345678901", 1000.00m, createdAt);

        // Assert
        customer.Id.Should().Be(id);
        customer.CreatedAtUtc.Should().Be(createdAt);
    }

    [Fact]
    public void Nome_WithLeadingAndTrailingSpaces_IsTrimmed()
    {
        // Arrange
        var nomeWithSpaces = "  João da Silva  ";

        // Act
        var customer = new Customer(nomeWithSpaces, "12345678901", 100.00m);

        // Assert
        customer.Nome.Should().Be("João da Silva");
    }

    [Fact]
    public void Id_IsAlwaysUnique()
    {
        // Act
        var customer1 = new Customer("Teste1", "12345678901", 100.00m);
        var customer2 = new Customer("Teste2", "12345678902", 100.00m);

        // Assert
        customer1.Id.Should().NotBe(customer2.Id);
    }
}
