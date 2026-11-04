using MicroFin.Transactions.Api.Application.Dtos;
using MicroFin.Transactions.Api.Application.Services;
using System.Security.Claims;

namespace MicroFin.Transactions.Api.Endpoints.Transactions;

/// <summary>
/// Endpoint para autorização de transações.
/// POST /api/v1/transactions/authorize
/// </summary>
public static class AuthorizeTransactionEndpoint
{
    public static void MapAuthorizeTransactionEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/transactions/authorize", async (
            AuthorizeRequest request,
            ITransactionService transactionService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            // Obter token JWT do header Authorization
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            var jwtToken = authHeader?.StartsWith("Bearer ") == true 
                ? authHeader[7..] 
                : authHeader;

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return Results.Unauthorized();
            }

            // Processar autorização
            var response = await transactionService.AuthorizeAsync(request, jwtToken, cancellationToken);

            if (response.Status == "APROVADO")
            {
                return Results.Ok(response);
            }
            else
            {
                return Results.Ok(response); // Retorna { "status": "NEGADO" }
            }
        })
        .WithName("AuthorizeTransaction")
        .WithOpenApi();
    }
}
