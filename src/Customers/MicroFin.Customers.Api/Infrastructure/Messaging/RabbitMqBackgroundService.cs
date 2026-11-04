using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MicroFin.Customers.Api.Application.Services;

namespace MicroFin.Customers.Api.Infrastructure.Messaging;

/// <summary>
/// Serviço de background para consumir mensagens RabbitMQ.
/// </summary>
public class RabbitMqBackgroundService : BackgroundService
{
    private readonly IRabbitMqConsumerService _consumerService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<RabbitMqBackgroundService>? _logger;
    private readonly string _queueName;

    public RabbitMqBackgroundService(
        IRabbitMqConsumerService consumerService,
        ICustomerService customerService,
        ILogger<RabbitMqBackgroundService>? logger,
        IConfiguration configuration)
    {
        _consumerService = consumerService;
        _customerService = customerService;
        _logger = logger;
        _queueName = configuration["RabbitMq:CustomersQueue"] ?? "microfin.customers.update-limit";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("Iniciando consumidor RabbitMQ para fila: {QueueName}", _queueName);

        _consumerService.StartConsuming(
            _queueName,
            ProcessMessageAsync,
            autoAck: false);

        return Task.CompletedTask;
    }

    private async Task<bool> ProcessMessageAsync(string message)
    {
        try
        {
            _logger?.LogDebug("Mensagem recebida");

            // Deserializar a mensagem
            var updateMessage = JsonSerializer.Deserialize<UpdateLimitMessage>(message);
            if (updateMessage == null)
            {
                _logger?.LogWarning("Mensagem inválida: não foi possível deserializar");
                return false;
            }

            // Processar a atualização do limite
            var success = await _customerService.ProcessUpdateLimitMessageAsync(
                updateMessage.MessageId,
                updateMessage.CustomerId,
                updateMessage.Amount);

            if (success)
            {
                _logger?.LogInformation(
                    "Limite atualizado com sucesso. CustomerId: {CustomerId}, Amount: {Amount}",
                    updateMessage.CustomerId,
                    updateMessage.Amount);
            }
            else
            {
                _logger?.LogWarning(
                    "Falha ao atualizar limite. CustomerId: {CustomerId}, Amount: {Amount}",
                    updateMessage.CustomerId,
                    updateMessage.Amount);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao processar mensagem RabbitMQ");
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Parando consumidor RabbitMQ");
        _consumerService.StopConsuming();
        await base.StopAsync(cancellationToken);
    }
}
