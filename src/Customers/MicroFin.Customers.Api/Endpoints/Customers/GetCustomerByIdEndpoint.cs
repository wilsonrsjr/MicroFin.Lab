using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroFin.Customers.Api.Application.Dtos;
using MicroFin.Customers.Api.Application.Services;

namespace MicroFin.Customers.Api.Endpoints.Customers;

/// <summary>
/// Endpoint para obtenção de cliente por ID.
/// </summary>
public static class GetCustomerByIdEndpoint
{
    /// <summary>
    /// Mapear o endpoint GET /api/v1/customers/{id}.
    /// </summary>
    /// <param name="app">Aplicação Web.</param>
    public static void MapGetCustomerByIdEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/customers/{id}", GetCustomerByIdAsync)
            .RequireAuthorization()
            .Produces<GetCustomerByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithName("GetCustomerById");
    }

    /// <summary>
    /// Handler para GET /api/v1/customers/{id}.
    /// </summary>
    private static async Task<IResult> GetCustomerByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ICustomerService customerService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await customerService.GetCustomerByIdAsync(id);

            if (response == null)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Status = "ERRO",
                    DetalheErro = "Cliente não encontrado."
                });
            }

            return Results.Ok(response);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
