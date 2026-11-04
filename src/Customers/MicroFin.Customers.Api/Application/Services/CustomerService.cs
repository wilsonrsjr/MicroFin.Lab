using Microsoft.Extensions.Caching.Memory;
using MicroFin.Customers.Api.Application.Dtos;
using MicroFin.Customers.Api.Domain;
using MicroFin.Customers.Api.Infrastructure.Data;

namespace MicroFin.Customers.Api.Application.Services;

/// <summary>
/// Serviço de negócio para clientes.
/// Responsável por validações, regras de negócio e coordenação entre componentes.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICpfValidationService _cpfValidationService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    private const string CacheKey = "customers_list";

    public CustomerService(
        ICustomerRepository customerRepository,
        ICpfValidationService cpfValidationService,
        IMemoryCache cache)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _cpfValidationService = cpfValidationService ?? throw new ArgumentNullException(nameof(cpfValidationService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    public async Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        // Validar CPF
        if (!_cpfValidationService.TryNormalizeAndValidate(request.Cpf, out var normalizedCpf))
        {
            throw new ArgumentException("CPF inválido.");
        }

        // Verificar se CPF já existe
        var existingCustomer = await _customerRepository.GetByCpfAsync(normalizedCpf);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException("CPF já cadastrado.");
        }

        // Validar valor do limite
        if (request.ValorLimite < 0)
        {
            throw new ArgumentException("Valor do limite não pode ser negativo.");
        }

        // Criar entidade de domínio
        var customer = new Customer(request.Nome, normalizedCpf, request.ValorLimite);

        // Persistir no banco
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Invalidar cache
        _cache.Remove(CacheKey);

        return new CreateCustomerResponse
        {
            IdCliente = createdCustomer.Id,
            Status = "OK"
        };
    }

    /// <summary>
    /// Obtém a lista de todos os clientes.
    /// </summary>
    public async Task<GetCustomersResponse> GetAllCustomersAsync()
    {
        // Tentar obter do cache
        if (_cache.TryGetValue(CacheKey, out GetCustomersResponse? cachedResponse))
        {
            return cachedResponse!;
        }

        // Buscar no banco
        var customers = await _customerRepository.GetAllAsync();

        // Mapear para DTO
        var customerListItems = customers.Select(c => new CustomerListItem
        {
            IdCliente = c.Id,
            Nome = c.Nome,
            Cpf = c.Cpf,
            ValorLimite = c.ValorLimite
        }).ToList();

        // Criar response
        var response = new GetCustomersResponse
        {
            Clientes = customerListItems,
            Status = "OK"
        };

        // Armazenar no cache
        _cache.Set(CacheKey, response, _cacheExpiration);

        return response;
    }

    /// <summary>
    /// Obtém um cliente pelo ID.
    /// </summary>
    public async Task<GetCustomerByIdResponse?> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        
        if (customer == null)
        {
            return null;
        }

        return new GetCustomerByIdResponse
        {
            IdCliente = customer.Id,
            Nome = customer.Nome,
            Cpf = customer.Cpf,
            ValorLimite = customer.ValorLimite,
            Status = "OK"
        };
    }

    /// <summary>
    /// Processa uma mensagem de atualização de limite.
    /// Garante atomicidade e idempotência.
    /// </summary>
    public async Task<bool> ProcessUpdateLimitMessageAsync(string messageId, Guid customerId, decimal amount)
    {
        // Verificar se a mensagem já foi processada
        var alreadyProcessed = await _customerRepository.IsMessageProcessedAsync(messageId);
        if (alreadyProcessed)
        {
            // Já processado, ignorar
            return true;
        }

        // Tentar atualizar limite de forma atômica
        var success = await _customerRepository.UpdateLimitAtomicAsync(customerId, amount);

        if (success)
        {
            // Registrar mensagem como processada
            await _customerRepository.RecordProcessedMessageAsync(messageId, customerId, amount);

            // Invalidar cache
            _cache.Remove(CacheKey);
        }

        return success;
    }
}
