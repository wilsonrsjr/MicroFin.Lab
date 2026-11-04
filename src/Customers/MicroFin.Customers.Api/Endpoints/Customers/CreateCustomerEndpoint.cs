using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroFin.Customers.Api.Application.Dtos;
using MicroFin.Customers.Api.Application.Services;

namespace MicroFin.Customers.Api.Endpoints.Customers;

/// <summary>
/// Endpoint para cadastro de clientes.
/// </summary>
public static class CreateCustomerEndpoint
{
    /// <summary>
    /// Mapear o endpoint POST /api/v1/customers.
    /// </summary>
    /// <param name="app">Aplicação Web.</param>
    public static void MapCreateCustomerEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/customers", CreateCustomerAsync)
            .RequireAuthorization()
            .Produces<CreateCustomerResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithName("CreateCustomer");
    }

    /// <summary>
    /// Handler para POST /api/v1/customers.
    /// </summary>
    private static async Task<IResult> CreateCustomerAsync(
        [FromBody] CreateCustomerRequest request,
        [FromServices] ICustomerService customerService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar request
            if (request == null)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Status = "ERRO",
                    DetalheErro = "Request não pode ser nulo."
                });
            }

            // Validar campos obrigatórios
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Nome))
                validationErrors.Add("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(request.Cpf))
                validationErrors.Add("CPF é obrigatório.");
            if (request.ValorLimite < 0)
                validationErrors.Add("Valor do limite não pode ser negativo.");

            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Status = "ERRO",
                    DetalheErro = string.Join(" ", validationErrors)
                });
            }

            // Criar cliente
            var response = await customerService.CreateCustomerAsync(request);

            // Retornar 201 Created
            return Results.Created(
                $"/api/v1/customers/{response.IdCliente}",
                response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse
            {
                Status = "ERRO",
                DetalheErro = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse
            {
                Status = "ERRO",
                DetalheErro = ex.Message
            });
        }
        catch (Exception)
        {
            // Erro genérico sem expor detalhes internos
            return Results.BadRequest(new ErrorResponse
            {
                Status = "ERRO",
                DetalheErro = "Erro ao processar a requisição."
            });
        }
    }
}
