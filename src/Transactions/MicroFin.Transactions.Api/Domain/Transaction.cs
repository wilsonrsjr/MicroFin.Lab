using System.ComponentModel.DataAnnotations;

namespace MicroFin.Transactions.Api.Domain;

/// <summary>
/// Representa uma transação de autorização de limite.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Identificador único da transação (GUID gerado automaticamente).
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// ID do cliente associado à transação.
    /// </summary>
    [Required(ErrorMessage = "ID do cliente é obrigatório.")]
    public Guid IdCliente { get; private set; }

    /// <summary>
    /// Valor da simulação/transação.
    /// </summary>
    [Required(ErrorMessage = "Valor da simulação é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor da simulação deve ser maior que zero.")]
    public decimal ValorSimulacao { get; private set; }

    /// <summary>
    /// Status da transação (APROVADO ou NEGADO).
    /// </summary>
    [Required(ErrorMessage = "Status é obrigatório.")]
    [StringLength(20, ErrorMessage = "Status deve ter no máximo 20 caracteres.")]
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    /// Data de criação da transação em UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Construtor padrão para serialização.
    /// </summary>
    public Transaction() { }

    /// <summary>
    /// Construtor para criação de uma nova transação.
    /// </summary>
    /// <param name="idCliente">ID do cliente.</param>
    /// <param name="valorSimulacao">Valor da simulação.</param>
    /// <param name="status">Status da transação (APROVADO/NEGADO).</param>
    public Transaction(Guid idCliente, decimal valorSimulacao, string status)
    {
        if (idCliente == Guid.Empty)
            throw new ArgumentException("ID do cliente não pode ser vazio.", nameof(idCliente));

        if (valorSimulacao <= 0)
            throw new ArgumentException("Valor da simulação deve ser maior que zero.", nameof(valorSimulacao));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status não pode ser nulo ou vazio.", nameof(status));

        if (status != "APROVADO" && status != "NEGADO")
            throw new ArgumentException("Status deve ser 'APROVADO' ou 'NEGADO'.", nameof(status));

        IdCliente = idCliente;
        ValorSimulacao = valorSimulacao;
        Status = status;
    }

    /// <summary>
    /// Método factory para criar transação aprovada.
    /// </summary>
    public static Transaction CreateApproved(Guid idCliente, decimal valorSimulacao)
    {
        return new Transaction(idCliente, valorSimulacao, "APROVADO");
    }

    /// <summary>
    /// Método factory para criar transação negada.
    /// </summary>
    public static Transaction CreateDenied(Guid idCliente, decimal valorSimulacao)
    {
        return new Transaction(idCliente, valorSimulacao, "NEGADO");
    }

    /// <summary>
    /// Método factory para criar transação com ID específico (para testes).
    /// </summary>
    public static Transaction CreateWithId(Guid id, Guid idCliente, decimal valorSimulacao, string status, DateTime createdAtUtc)
    {
        var transaction = new Transaction(idCliente, valorSimulacao, status)
        {
            Id = id,
            CreatedAtUtc = createdAtUtc
        };
        return transaction;
    }
}
