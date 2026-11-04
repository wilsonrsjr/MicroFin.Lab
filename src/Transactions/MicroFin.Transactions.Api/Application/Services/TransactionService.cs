using MicroFin.Transactions.Api.Application.Dtos;
using MicroFin.Transactions.Api.Domain;
using MicroFin.Transactions.Api.Infrastructure.Data;
using MicroFin.Transactions.Api.Infrastructure.Messaging;

namespace MicroFin.Transactions.Api.Application.Services;

/// <summary>
/// Serviço de processamento de transações.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICustomerClientService _customerClientService;
    private readonly IRabbitMqPublisherService _rabbitMqPublisherService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICustomerClientService customerClientService,
        IRabbitMqPublisherService rabbitMqPublisherService)
    {
        _transactionRepository = transactionRepository;
        _customerClientService = customerClientService;
        _rabbitMqPublisherService = rabbitMqPublisherService;
    }

    /// <summary>
    /// Processa uma solicitação de autorização de transação.
    /// </summary>
    public async Task<AuthorizeResponse> AuthorizeAsync(AuthorizeRequest request, string jwtToken, CancellationToken cancellationToken = default)
    {
        // 1. Validar entrada
        if (request == null)
        {
            return new AuthorizeResponse { Status = "NEGADO" };
        }

        // 2. Obter cliente via API Customers
        var customerResponse = await _customerClientService.GetCustomerByIdAsync(request.IdCliente, jwtToken, cancellationToken);
        
        if (customerResponse == null || customerResponse.IdCliente == Guid.Empty)
        {
            // Cliente não existe
            return new AuthorizeResponse { Status = "NEGADO" };
        }

        // 3. Verificar limite disponível
        if (customerResponse.ValorLimite < request.ValorSimulacao)
        {
            // Limite insuficiente
            return new AuthorizeResponse { Status = "NEGADO" };
        }

        // 4. Verificar valor válido
        if (request.ValorSimulacao <= 0)
        {
            return new AuthorizeResponse { Status = "NEGADO" };
        }

        // 5. Gerar GUID da transação
        var idTransacao = Guid.NewGuid();

        // 6. Persistir transação localmente (para idempotência local)
        var transaction = Transaction.CreateWithId(
            idTransacao,
            request.IdCliente,
            request.ValorSimulacao,
            "APROVADO",
            DateTime.UtcNow);

        await _transactionRepository.CreateAsync(transaction);

        // 7. Publicar mensagem para Customers via RabbitMQ
        var message = new
        {
            messageId = idTransacao.ToString(),
            customerId = request.IdCliente.ToString(),
            amount = request.ValorSimulacao
        };

        await _rabbitMqPublisherService.PublishAsync(
            exchange: "microfin.transactions",
            routingKey: "transaction.authorized",
            queue: "microfin.customers.update-limit",
            message);

        // 8. Retornar aprovação
        return new AuthorizeResponse
        {
            Status = "APROVADO",
            IdTransacao = idTransacao
        };
    }
}
