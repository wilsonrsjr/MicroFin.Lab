# MicroFin.Lab

**Solução completa com três APIs REST independentes em .NET 10 para gestão de autenticação, clientes e autorização de transações.**

- `MicroFin.Auth.Api` — API de Autenticação (Porta: 5273)
- `MicroFin.Customers.Api` — API de Clientes (Porta: 5264)
- `MicroFin.Transactions.Api` — API de Transações (Porta: 5080)

---

## 📋 Índice

- [Pré-requisitos](#pr%C3%A9-requisitos)
- [Configuração](#configura%C3%A7%C3%A3o)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Endpoints](#endpoints)
- [Fluxo de Autorização](#fluxo-de-autoriza%C3%A7%C3%A3o)
- [Testes](#testes)
- [Comandos](#comandos)
- [Execução](#execu%C3%A7%C3%A3o)
- [Validação Manual](#valida%C3%A7%C3%A3o-manual)
- [Tecnologias](#tecnologias)
- [Critérios de Aceite](#crit%C3%A9rios-de-aceite)
- [Limitações Conhecidas](#limita%C3%A7%C3%B5es-conhecidas)

---

## 🎯 Pré-requisitos

### Software

- **.NET SDK** 10.0.401 ou superior (recomendado: fixado em `global.json`)
- **SQL Server Express** 2022 ou superior (instância local em `127.0.0.1:1434`)
- **RabbitMQ** 3.12+ (servidor local em `127.0.0.1:5672`)

### Infraestrutura Local Existente

A solução depende dos seguintes serviços locais:

| Serviço | Endereço | Porta | Observações |
|---------|----------|-------|-------------|
| SQL Server | `127.0.0.1` | 1434 | Instância SQL Server Express |
| RabbitMQ | `127.0.0.1` | 5672 | Default credentials |
| RabbitMQ Management | `127.0.0.1` | 15672 | Interface web |

---

### 2. Criar Banco de Dados

Execute os scripts SQL para criar as tabelas necessárias:

---

### 3. Verificar RabbitMQ

Acesse a interface de administração do RabbitMQ:
- URL: http://127.0.0.1:15672
- Usuário: `guest`
- Senha: `guest`

Verifique que as filas e exchanges existem:
- Exchange: `microfin.transactions`
- Queue: `microfin.customers.update-limit`
- Routing Key: `transaction.authorized`

---

## 🔌 Endpoints

### Auth API (http://localhost:5273)

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|---------------|
| POST | `/api/v1/auth/register` | Cadastro de usuário | ❌ Não requer |
| POST | `/api/v1/auth/login` | Login e geração de JWT | ❌ Não requer |

#### Register

**Request:**
```json
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@microfin.com",
  "password": "StrongPassword123!"
}
```

**Response (201 Created):**
```json
{
  "id": "e8b97d10-8b1e-4c3a-96e2-2a7fbf6680a1",
  "email": "user@microfin.com",
  "message": "Usuário cadastrado com sucesso."
}
```

#### Login

**Request:**
```json
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@microfin.com",
  "password": "StrongPassword123!"
}
```

**Response (200 OK):**
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresInSeconds": 3600
}
```

---

### Customers API (http://localhost:5264)

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|---------------|
| POST | `/api/v1/customers` | Cadastro de cliente | ✅ JWT Bearer |
| GET | `/api/v1/customers` | Listagem de clientes | ✅ JWT Bearer |
| GET | `/api/v1/customers/{id}` | Obter cliente por ID | ✅ JWT Bearer |

#### Create Customer

**Request:**
```json
POST /api/v1/customers
Content-Type: application/json
Authorization: Bearer <accessToken>

{
  "nome": "João da Silva",
  "cpf": "12345678901",
  "valorLimite": 1500.00
}
```

**Response (201 Created):**
```json
{
  "idCliente": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "OK"
}
```

**Response (400 Bad Request - CPF duplicado):**
```json
{
  "status": "ERRO",
  "detalheErro": "CPF já cadastrado."
}
```

#### Get All Customers

**Request:**
```
GET /api/v1/customers
Authorization: Bearer <accessToken>
```

**Response (200 OK):**
```json
[
  {
    "idCliente": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João da Silva",
    "cpf": "12345678901",
    "valorLimite": 1500.00,
    "status": "OK"
  }
]
```

> 💡 **Cache**: A listagem é cacheada por 10 minutos via `IMemoryCache`.

#### Get Customer by ID

**Request:**
```
GET /api/v1/customers/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <accessToken>
```

**Response (200 OK):**
```json
{
  "idCliente": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "João da Silva",
  "cpf": "12345678901",
  "valorLimite": 1500.00,
  "status": "OK"
}
```

---

### Transactions API (http://localhost:5080)

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|---------------|
| POST | `/api/v1/transactions/authorize` | Autorização de transação | ✅ JWT Bearer |

#### Authorize Transaction

**Request:**
```json
POST /api/v1/transactions/authorize
Content-Type: application/json
Authorization: Bearer <accessToken>

{
  "idCliente": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "valorSimulacao": 250.50
}
```

**Response (200 OK - Aprovado):**
```json
{
  "status": "APROVADO",
  "idTransacao": "9cf0111b-c8c1-4d51-857f-0135650ad586"
}
```

**Response (200 OK - Negado):**
```json
{
  "status": "NEGADO"
}
```

> ⚠️ **Negado quando**: Cliente não existe, limite insuficiente, ou valor inválido.

---

## 🧪 Testes

### 🧭 Validação Manual

### 1. Testar Cadastro de Usuário

```bash
# Cadastrar usuário
curl -X POST http://localhost:5273/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "test@microfin.com", "password": "Test@123456"}'

# Verificar resposta
# Expected: 201 Created
```

### 2. Testar Login e JWT

```bash
# Login
TOKEN=$(curl -X POST http://localhost:5273/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@microfin.com", "password": "Test@123456"}' \
  -H "Accept: application/json" | jq -r '.accessToken')

echo "Token JWT: $TOKEN"

# Verificar expiração (deve ser 3600 segundos = 1 hora)
curl -X POST http://localhost:5273/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@microfin.com", "password": "Test@123456"}' | jq '.expiresInSeconds'
```

### 3. Testar JWT com Credenciais Inválidas

```bash
# Senha incorreta
curl -X POST http://localhost:5273/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@microfin.com", "password": "senha_errada"}'

# Expected: 401 Unauthorized ou resposta com erro
```

### 4. Testar Cadastrar Cliente

```bash
# Cadastrar cliente (requer JWT)
curl -X POST http://localhost:5264/api/v1/customers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"nome": "João da Silva", "cpf": "12345678901", "valorLimite": 1500.00}'

# Verificar resposta
# Expected: 201 Created
# {"idCliente": "...", "status": "OK"}
```

### 5. Testar Cliente Duplicado

```bash
# Tentar cadastrar mesmo CPF
curl -X POST http://localhost:5264/api/v1/customers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"nome": "Maria Santos", "cpf": "12345678901", "valorLimite": 2000.00}'

# Expected: 400 Bad Request ou erro de CPF duplicado
```

### 6. Testar Limite Negativo

```bash
# Tentar cadastrar com limite negativo
curl -X POST http://localhost:5264/api/v1/customers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"nome": "Teste Negativo", "cpf": "99999999999", "valorLimite": -100.00}'

# Expected: 400 Bad Request ou erro de valor inválido
```

### 7. Testar Listagem de Clientes

```bash
# Listar clientes
curl -X GET http://localhost:5264/api/v1/customers \
  -H "Authorization: Bearer $TOKEN"

# Expected: 200 OK com lista de clientes
```

### 8. Testar Transação - Cliente Inexistente

```bash
# Tentar autorizar com ID inexistente
curl -X POST http://localhost:5080/api/v1/transactions/authorize \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"idCliente": "00000000-0000-0000-0000-000000000000", "valorSimulacao": 100.00}'

# Expected: {"status": "NEGADO"}
```

### 9. Testar Transação - Aprovada

```bash
# Obter ID do cliente criado
CLIENTE_ID=$(curl -X GET http://localhost:5264/api/v1/customers \
  -H "Authorization: Bearer $TOKEN" | jq -r '.[0].idCliente')

echo "ID do Cliente: $CLIENTE_ID"

# Autorizar transação válida
curl -X POST http://localhost:5080/api/v1/transactions/authorize \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"idCliente\": \"$CLIENTE_ID\", \"valorSimulacao\": 100.00}"

# Expected: {"status": "APROVADO", "idTransacao": "..."}
```

### 10. Testar Transação - Limite Insuficiente

```bash
# Autorizar transação com valor maior que limite
curl -X POST http://localhost:5080/api/v1/transactions/authorize \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"idCliente\": \"$CLIENTE_ID\", \"valorSimulacao\": 10000.00}"

# Expected: {"status": "NEGADO"}
```

### 11. Verificar Débito do Limite

```bash
# Verificar que o limite foi debitado
curl -X GET http://localhost:5264/api/v1/customers/$CLIENTE_ID \
  -H "Authorization: Bearer $TOKEN"

# Expected: valorLimite deve ser 1400.00 (1500.00 - 100.00)
```

### 12. Testar JWT Inválido em Customers e Transactions

```bash
# Tentar acessar Customers com token inválido
curl -X GET http://localhost:5264/api/v1/customers \
  -H "Authorization: Bearer invalid-token-123"

# Expected: 401 Unauthorized

# Tentar acessar Transactions com token inválido
curl -X POST http://localhost:5080/api/v1/transactions/authorize \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer invalid-token-123" \
  -d '{"idCliente": "...", "valorSimulacao": 100.00}'

# Expected: 401 Unauthorized
```

### 13. Verificar Processamento de Mensagem RabbitMQ

1. Acesse http://127.0.0.1:15672 (guest/guest)
2. Verifique a fila `microfin.customers.update-limit`
3. Após autorizar uma transação, a mensagem deve aparecer e ser consumida automaticamente

---

## 🛡️ Tecnologias

| Categoria | Tecnologia | Versão | Uso |
|-----------|------------|--------|-----|
| Runtime | .NET | 10.0 | APIs REST |
| Linguagem | C# | 10 | Backend |
| Web Framework | ASP.NET Core | 10.0 | Web API |
| ORM | Dapper | 2.1.66 | Acesso a dados |
| Banco de Dados | SQL Server | 2022+ | Persistência |
| Mensageria | RabbitMQ | 3.12+ | Comunicação assíncrona |
| Cache | IMemoryCache | - | Cache local |
| Autenticação | JWT Bearer | - | Segurança |
| Testes | xUnit | 2.9.3 | Testes unitários |
| Testes | Moq | 4.20.72 | Mocking |
