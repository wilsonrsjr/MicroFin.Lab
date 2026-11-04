using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using MicroFin.Customers.Api.Application.Services;
using MicroFin.Customers.Api.Infrastructure.Data;
using MicroFin.Customers.Api.Infrastructure.Messaging;
using MicroFin.Customers.Api.Endpoints.Customers;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("application", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Configurar MemoryCache
builder.Services.AddMemoryCache();

// Registrar serviços de aplicação
builder.Services.AddScoped<ICpfValidationService, CpfValidationService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Registrar repositório
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Configurar RabbitMQ
builder.Services.AddRabbitMqConsumer(builder.Configuration);

// Configurar autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurado. Use User Secrets ou variáveis de ambiente.");

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Chave JWT deve ter pelo menos 32 caracteres para segurança.");
}

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MicroFin.Auth",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MicroFin.Apis",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configurar connection string (para uso do Dapper)
// O CustomerRepository já lê do IConfiguration

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// Mapear endpoints de clientes
app.MapCreateCustomerEndpoint();
app.MapGetCustomersEndpoint();
app.MapGetCustomerByIdEndpoint();

app.Run();

public partial class Program;
