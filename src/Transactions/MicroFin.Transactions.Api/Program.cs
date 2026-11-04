using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using MicroFin.Transactions.Api.Infrastructure.Data;
using MicroFin.Transactions.Api.Infrastructure.Messaging;
using MicroFin.Transactions.Api.Application.Services;
using MicroFin.Transactions.Api.Endpoints.Transactions;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication - JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Chave JWT não configurada. Configure via User Secrets ou variáveis de ambiente.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "MicroFin.Auth",
            ValidAudience = jwtSettings["Audience"] ?? "MicroFin.Apis",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("application", () => HealthCheckResult.Healthy());

// HttpClient para chamadas à API Customers
builder.Services.AddHttpClient<ICustomerClientService, CustomerClientService>();

// Database connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

// Dependency Injection
builder.Services.AddSingleton<IRabbitMqPublisherService, RabbitMqPublisherService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICustomerClientService, CustomerClientService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// Endpoints
app.MapAuthorizeTransactionEndpoint();

app.Run();

public partial class Program;
