using MicroFin.Transactions.Api.Application.Dtos;

namespace MicroFin.Transactions.Api.Application.Services;

/// <summary>
/// Interface do serviço de transações.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Autoriza uma transação.
    /// </summary>
    Task<AuthorizeResponse> AuthorizeAsync(AuthorizeRequest request, string jwtToken, CancellationToken cancellationToken = default);
}
