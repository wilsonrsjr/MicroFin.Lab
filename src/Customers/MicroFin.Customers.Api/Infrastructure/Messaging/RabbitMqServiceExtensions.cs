namespace MicroFin.Customers.Api.Infrastructure.Messaging;

/// <summary>
/// Extensões para registrar serviços RabbitMQ.
/// </summary>
public static class RabbitMqServiceExtensions
{
    /// <summary>
    /// Adiciona o consumidor RabbitMQ como um background service.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração.</param>
    /// <returns>Coleção de serviços atualizada.</returns>
    public static IServiceCollection AddRabbitMqConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRabbitMqConsumerService, RabbitMqConsumerService>();
        services.AddHostedService<RabbitMqBackgroundService>();
        return services;
    }
}
