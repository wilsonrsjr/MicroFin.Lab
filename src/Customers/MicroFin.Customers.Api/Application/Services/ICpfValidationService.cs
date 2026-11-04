namespace MicroFin.Customers.Api.Application.Services;

/// <summary>
/// Interface para serviço de validação de CPF.
/// </summary>
public interface ICpfValidationService
{
    /// <summary>
    /// Valida se um CPF é válido.
    /// </summary>
    /// <param name="cpf">CPF a ser validado (com ou sem formatação).</param>
    /// <returns>True se o CPF é válido; false caso contrário.</returns>
    bool IsValid(string cpf);

    /// <summary>
    /// Valida e normaliza um CPF (remove formatação e valida).
    /// </summary>
    /// <param name="cpf">CPF a ser validado e normalizado.</param>
    /// <param name="normalizedCpf">CPF normalizado (apenas 11 dígitos).</param>
    /// <returns>True se o CPF é válido; false caso contrário.</returns>
    bool TryNormalizeAndValidate(string cpf, out string normalizedCpf);
}
