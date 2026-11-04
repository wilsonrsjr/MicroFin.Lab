using System.Net.Http.Headers;
using System.Text.Json;

namespace MicroFin.Transactions.Api.Application.Services;

/// <summary>
/// Implementação do cliente HTTP para a API de Customers.
/// </summary>
public class CustomerClientService : ICustomerClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public CustomerClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = configuration["CustomersApi:BaseUrl"] ?? "http://localhost:5002";
    }

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid customerId, string jwtToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new ArgumentException("Token JWT é obrigatório.", nameof(jwtToken));
        }

        // Configurar header de autorização
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/customers/{customerId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<CustomerResponse>(json, options);
            }

            // Se não encontrado,ornar null
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            // Outro erro - lançar exceção
            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception)
        {
            // Em caso de erro de comunicação, retornamos null
            // para que o serviço de transações trate como cliente não encontrado
            return null;
        }
    }
}
