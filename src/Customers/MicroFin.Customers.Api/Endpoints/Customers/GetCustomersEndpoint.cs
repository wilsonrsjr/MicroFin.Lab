using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroFin.Customers.Api.Application.Dtos;
using MicroFin.Customers.Api.Application.Services;

namespace MicroFin.Customers.Api.Endpoints.Customers;

/// <summary>
/// Endpoint para listagem de clientes.
/// </summary>
public static class GetCustomersEndpoint
{
    /// <summary>
    /// Mapear o endpoint GET /api/v1/customers.
    /// </summary>
    /// <param name="app">Aplicação Web.</param>
    public static void MapGetCustomersEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/customers", GetCustomersAsync)
            .RequireAuthorization()
            .Produces<GetCustomersResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithName("GetCustomers");
    }

    /// <summary>
    /// Handler para GET /api/v1/customers.
    /// </summary>
    private static async Task<IResult> GetCustomersAsync(
        [FromServices] ICustomerService customerService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Obter clientes
            var response = await customerService.GetAllCustomersAsync();

            // Retornar 200 OK
            return Results.Ok(response);
        }
        catch (Exception)
        {
            // Erro genérico sem expor detalhes internos
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
