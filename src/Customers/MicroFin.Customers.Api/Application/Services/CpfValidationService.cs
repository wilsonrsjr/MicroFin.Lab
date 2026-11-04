namespace MicroFin.Customers.Api.Application.Services;

/// <summary>
/// Serviço de validação de CPF (Cadastro de Pessoas Físicas).
/// Implementa o algoritmo oficial de validação de CPF do Brasil.
/// </summary>
public class CpfValidationService : ICpfValidationService
{
    private const int CpfLength = 11;

    /// <summary>
    /// Valida se um CPF é válido.
    /// </summary>
    /// <param name="cpf">CPF a ser validado (com ou sem formatação).</param>
    /// <returns>True se o CPF é válido; false caso contrário.</returns>
    public bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remover caracteres não numéricos
        var cleaned = new string(cpf.Where(char.IsDigit).ToArray());

        // Deve ter exatamente 11 dígitos
        if (cleaned.Length != CpfLength)
            return false;

        // CPF inválido se todos os dígitos forem iguais (ex: 11111111111)
        if (cleaned.All(c => c == cleaned[0]))
            return false;

        // Validar dígitos verificadores
        return ValidateCheckDigits(cleaned);
    }

    /// <summary>
    /// Valida e normaliza um CPF.
    /// </summary>
    /// <param name="cpf">CPF a ser validado e normalizado.</param>
    /// <param name="normalizedCpf">CPF normalizado (apenas 11 dígitos).</param>
    /// <returns>True se o CPF é válido; false caso contrário.</returns>
    public bool TryNormalizeAndValidate(string cpf, out string normalizedCpf)
    {
        normalizedCpf = string.Empty;

        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var cleaned = new string(cpf.Where(char.IsDigit).ToArray());

        if (cleaned.Length != CpfLength)
            return false;

        if (cleaned.All(c => c == cleaned[0]))
            return false;

        if (!ValidateCheckDigits(cleaned))
            return false;

        normalizedCpf = cleaned;
        return true;
    }

    /// <summary>
    /// Valida os dígitos verificadores do CPF.
    /// </summary>
    /// <param name="cpfDigits">CPF com 11 dígitos numéricos.</param>
    /// <returns>True se os dígitos verificadores são válidos.</returns>
    private static bool ValidateCheckDigits(string cpfDigits)
    {
        // Calcular primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (cpfDigits[i] - '0') * (10 - i);
        }

        int digit1 = (sum % 11) < 2 ? 0 : 11 - (sum % 11);

        if (digit1 != (cpfDigits[9] - '0'))
            return false;

        // Calcular segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += (cpfDigits[i] - '0') * (11 - i);
        }

        int digit2 = (sum % 11) < 2 ? 0 : 11 - (sum % 11);

        return digit2 == (cpfDigits[10] - '0');
    }
}
