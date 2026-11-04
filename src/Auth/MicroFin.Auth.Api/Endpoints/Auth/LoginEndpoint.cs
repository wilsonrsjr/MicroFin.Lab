using MicroFin.Auth.Api.Application.Dtos;
using MicroFin.Auth.Api.Application.Services;
using MicroFin.Auth.Api.Infrastructure.Data;

namespace MicroFin.Auth.Api.Endpoints.Auth;

/// <summary>
/// Endpoint para autenticação de usuários existentes e geração de JWT.
/// </summary>
public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/login", HandleLogin)
            .WithName("LoginUser")
            .WithOpenApi()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleLogin(
        LoginRequest request,
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService,
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

        try
        {
            // Buscar usuário por e-mail
            var user = await userRepository.FindByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                // Por segurança, não informar se o e-mail existe ou não
                return Results.Unauthorized();
            }

            // Verificar senha
            var isPasswordValid = passwordHashService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                // Por segurança, não informar diferença entre e-mail inexistente e senha incorreta
                return Results.Unauthorized();
            }

            // Gerar JWT
            var token = jwtTokenService.GenerateToken(user.Id, user.Email);

            var response = new LoginResponse
            {
                TokenType = "Bearer",
                AccessToken = token,
                ExpiresInSeconds = jwtTokenService.TokenExpirationSeconds
            };

            return Results.Ok(response);
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
}
