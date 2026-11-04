using MicroFin.Auth.Api.Application.Dtos;
using MicroFin.Auth.Api.Application.Services;
using MicroFin.Auth.Api.Domain;
using MicroFin.Auth.Api.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MicroFin.Auth.Api.Endpoints.Auth;

/// <summary>
/// Endpoint para registro de novos usuários.
/// </summary>
public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/register", HandleRegister)
            .WithName("RegisterUser")
            .WithOpenApi()
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleRegister(
        RegisterRequest request,
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        CancellationToken cancellationToken)
    {
        // Validação de entrada
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { erro = "E-mail é obrigatório." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { erro = "Senha é obrigatória." });
        }

        // Validar formato de e-mail
        if (!IsValidEmail(request.Email))
        {
            return Results.BadRequest(new { erro = "Formato de e-mail inválido." });
        }

        // Validar força da senha (mínimo 8 caracteres)
        if (request.Password.Length < 8)
        {
            return Results.BadRequest(new { erro = "Senha deve ter pelo menos 8 caracteres." });
        }

        // Verificar se e-mail já está registrado
        var existingUser = await userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Results.Conflict(new { erro = "Este e-mail já está registrado." });
        }

        try
        {
            // Gerar hash da senha
            var passwordHash = passwordHashService.HashPassword(request.Password);

            // Criar novo usuário
            var newUser = new User(request.Email, passwordHash);

            // Persistir
            var created = await userRepository.CreateAsync(newUser, cancellationToken);
            if (created == null)
            {
                return Results.Conflict(new { erro = "Este e-mail já está registrado." });
            }

            // Retornar response sem expor hash
            var response = new RegisterResponse
            {
                Id = created.Id,
                Email = created.Email,
                Message = "Usuário cadastrado com sucesso."
            };

            return Results.Created($"/api/v1/auth/users/{created.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { erro = ex.Message });
        }
        catch (Exception ex)
        {
            // Log não deve expor detalhes internos
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
