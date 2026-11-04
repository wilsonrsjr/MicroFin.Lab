using System.ComponentModel.DataAnnotations;

namespace MicroFin.Customers.Api.Domain;

/// <summary>
/// Representa um cliente do sistema MicroFin.Lab.
/// </summary>
public class Customer
{
    /// <summary>
    /// Identificador único do cliente (GUID gerado automaticamente).
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Nome completo do cliente.
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres.")]
    public string Nome { get; private set; } = string.Empty;

    /// <summary>
    /// CPF do cliente (apenas dígitos numéricos, 11 caracteres).
    /// </summary>
    [Required(ErrorMessage = "CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos numéricos.")]
    public string Cpf { get; private set; } = string.Empty;

    /// <summary>
    /// Valor do limite disponível para o cliente.
    /// </summary>
    [Required(ErrorMessage = "Valor do limite é obrigatório.")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor do limite não pode ser negativo.")]
    public decimal ValorLimite { get; private set; }

    /// <summary>
    /// Data de criação do registro em UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização em UTC (nulo para novos registros).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Construtor padrão para serialização.
    /// </summary>
    public Customer() { }

    /// <summary>
    /// Construtor para criação de um novo cliente.
    /// </summary>
    /// <param name="nome">Nome do cliente.</param>
    /// <param name="cpf">CPF do cliente (apenas dígitos).</param>
    /// <param name="valorLimite">Valor do limite (deve ser >= 0).</param>
    public Customer(string nome, string cpf, decimal valorLimite)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser nulo ou vazio.", nameof(nome));

        if (nome.Length > 255)
            throw new ArgumentException("Nome deve ter no máximo 255 caracteres.", nameof(nome));

        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser nulo ou vazio.", nameof(cpf));

        var cleanedCpf = new string(cpf.Where(char.IsDigit).ToArray());
        if (cleanedCpf.Length != 11)
            throw new ArgumentException("CPF deve conter exatamente 11 dígitos numéricos.", nameof(cpf));

        if (valorLimite < 0)
            throw new ArgumentException("Valor do limite não pode ser negativo.", nameof(valorLimite));

        Nome = nome.Trim();
        Cpf = cleanedCpf;
        ValorLimite = valorLimite;
    }

    /// <summary>
    /// Atualiza o limite do cliente subtraindo um valor.
    /// </summary>
    /// <param name="amount">Valor a ser debitado.</param>
    /// <exception cref="ArgumentOutOfRangeException">Se o valor for negativo.</exception>
    /// <exception cref="InvalidOperationException">Se o limite resultante for negativo.</exception>
    public void DebitLimit(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Valor a debitar não pode ser negativo.");

        if (ValorLimite < amount)
            throw new InvalidOperationException("Limite insuficiente para debitar o valor solicitado.");

        ValorLimite -= amount;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Método factory para criar cliente com ID específico (para testes ou persistência).
    /// </summary>
    public static Customer CreateWithId(Guid id, string nome, string cpf, decimal valorLimite, DateTime createdAtUtc)
    {
        var customer = new Customer(nome, cpf, valorLimite)
        {
            Id = id,
            CreatedAtUtc = createdAtUtc
        };
        return customer;
    }
}
