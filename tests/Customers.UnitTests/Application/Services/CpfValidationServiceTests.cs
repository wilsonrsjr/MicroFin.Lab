using FluentAssertions;
using MicroFin.Customers.Api.Application.Services;
using Xunit;

namespace MicroFin.Customers.Api.Tests.Unit.Application.Services;

/// <summary>
/// Testes unitários para o serviço de validação de CPF.
/// </summary>
public class CpfValidationServiceTests
{
    private readonly ICpfValidationService _service = new CpfValidationService();

    [Theory]
    [InlineData("12345678909")] // CPF de teste válido
    [InlineData("52998224725")] // CPF de teste válido
    [InlineData("11144477735")] // CPF de teste válido
    [InlineData("123.456.789-09")] // CPF válido com formatação
    [InlineData("123-456-789-09")] // CPF válido com traços
    public void IsValid_WithValidCpf_ReturnsTrue(string cpf)
    {
        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")] // Muito curto
    [InlineData("123456789012")] // Muito longo
    [InlineData("11111111111")] // Todos os dígitos iguais
    [InlineData("00000000000")] // Todos zeros
    [InlineData("abcdefghijk")] // Caracteres não numéricos
    [InlineData("1234567890A")] // Caractere não numérico
    public void IsValid_WithInvalidCpf_ReturnsFalse(string cpf)
    {
        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithFormattedCpf_ValidatesCorrectly()
    {
        // Arrange
        var formattedCpf = "529.982.247-25";

        // Act
        var result = _service.IsValid(formattedCpf);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryNormalizeAndValidate_WithValidFormattedCpf_ReturnsTrueAndNormalizedCpf()
    {
        // Arrange
        var formattedCpf = "529.982.247-25";

        // Act
        var result = _service.TryNormalizeAndValidate(formattedCpf, out var normalizedCpf);

        // Assert
        result.Should().BeTrue();
        normalizedCpf.Should().Be("52998224725");
    }

    [Fact]
    public void TryNormalizeAndValidate_WithValidUnformattedCpf_ReturnsTrueAndSameCpf()
    {
        // Arrange
        var unformattedCpf = "52998224725";

        // Act
        var result = _service.TryNormalizeAndValidate(unformattedCpf, out var normalizedCpf);

        // Assert
        result.Should().BeTrue();
        normalizedCpf.Should().Be(unformattedCpf);
    }

    [Fact]
    public void TryNormalizeAndValidate_WithInvalidCpf_ReturnsFalseAndEmptyString()
    {
        // Arrange
        var invalidCpf = "123";

        // Act
        var result = _service.TryNormalizeAndValidate(invalidCpf, out var normalizedCpf);

        // Assert
        result.Should().BeFalse();
        normalizedCpf.Should().BeEmpty();
    }

    [Fact]
    public void TryNormalizeAndValidate_WithNullCpf_ReturnsFalseAndEmptyString()
    {
        // Act
        var result = _service.TryNormalizeAndValidate(null!, out var normalizedCpf);

        // Assert
        result.Should().BeFalse();
        normalizedCpf.Should().BeEmpty();
    }

    [Fact]
    public void TryNormalizeAndValidate_WithAllSameDigits_ReturnsFalse()
    {
        // Arrange
        var allSameDigits = "11111111111";

        // Act
        var result = _service.TryNormalizeAndValidate(allSameDigits, out var normalizedCpf);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryNormalizeAndValidate_WithSpacesAndSpecialChars_ReturnsTrue()
    {
        // Arrange
        var cpfWithChars = " 529 . 982 . 247 - 25 ";

        // Act
        var result = _service.TryNormalizeAndValidate(cpfWithChars, out var normalizedCpf);

        // Assert
        result.Should().BeTrue();
        normalizedCpf.Should().Be("52998224725");
    }

    // Testes com CPFs conhecidos válidos
    private static readonly string[] ValidCpfs = 
    {
        "12345678909",
        "52998224725",
        "11144477735"
    };

    [Theory]
    [MemberData(nameof(GetValidCpfsTheoryData))]
    public void IsValid_WithKnownValidCpfs_ReturnsTrue(string cpf)
    {
        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetValidCpfsTheoryData()
    {
        foreach (var cpf in ValidCpfs)
        {
            yield return new object[] { cpf };
        }
    }

    // Testes com CPFs com formatação variada
    [Theory]
    [InlineData("123.456.789-09")]
    [InlineData("123456789-09")]
    [InlineData("123-456-789-09")]
    [InlineData("123 456 789 09")]
    public void IsValid_WithVariousFormats_ReturnsTrue(string cpf)
    {
        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithCpfStartingWithZero_ReturnsTrue()
    {
        // CPF válido que começa com zero
        var cpf = "00000000191";

        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidCheckDigit1_ReturnsFalse()
    {
        // CPF com primeiro dígito verificador inválido
        var cpf = "12345678900"; // Dígito 1 inválido (deveria ser 0)

        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidCheckDigit2_ReturnsFalse()
    {
        // CPF com segundo dígito verificador inválido
        var cpf = "12345678900"; // Dígito 2 inválido

        // Act
        var result = _service.IsValid(cpf);

        // Assert
        result.Should().BeFalse();
    }
}
