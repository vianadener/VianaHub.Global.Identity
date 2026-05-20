# Identity API

API de autenticação e autorização multi-tenant baseada em **JWT com chaves RSA por tenant**, **RBAC** (Role-Based Access Control) e **Refresh Token com rotação automática**.

Construída com **.NET 8**, seguindo **Arquitetura Hexagonal**, **DDD**, **SOLID** e **Clean Architecture**.

---

## Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Variáveis e Configuração](#variáveis-e-configuração)
- [Banco de Dados](#banco-de-dados)
- [Seed de Dados](#seed-de-dados)
- [Execução Local](#execução-local)
- [Autenticação](#autenticação)
- [Endpoints](#endpoints)
- [Exemplos de Chamadas](#exemplos-de-chamadas)

---

## Visão Geral

O Identity API é o serviço central de identidade da plataforma EBL FIG. Ele gerencia:

- **Multi-tenancy**: cada tenant possui seus próprios usuários, roles, permissões e chaves JWT RSA isoladas.
- **RBAC stateless**: permissões são embutidas no JWT — nenhuma consulta ao banco durante a autorização.
- **Rotação de Refresh Token**: cada uso gera um novo token e revoga o anterior.
- **Chaves JWT RSA por tenant**: cada tenant possui um par de chaves RSA (privada criptografada em banco, pública para validação).
- **Hangfire**: agendamento de jobs com filas dinâmicas por banco de dados.

---

## Arquitetura

```
Identity.Api/
├── src/
│   ├── EBL.FIG.Process.Identity.Api           # Camada de entrada (Endpoints, Middleware, Configuração)
│   ├── EBL.FIG.Process.Identity.Application   # Casos de uso, DTOs, Interfaces de Application
│   ├── EBL.FIG.Process.Identity.Domain        # Entidades, Value Objects, Interfaces de repositório, Serviços de domínio
│   ├── EBL.FIG.Process.Identity.Infra.Data    # EF Core, Repositórios, Seeders, Context, Interceptores
│   ├── EBL.FIG.Process.Identity.Infra.IoC     # Registro de dependências
│   ├── EBL.FIG.Process.Identity.Infra.Integration  # Integrações externas
│   └── EBL.FIG.Process.Identity.Infra.Job     # Jobs Hangfire
└── tests/
    └── EBL.FIG.Process.Identity.Tests
```

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 8.0 |
| SQL Server | 2019 ou superior (Express funciona) |
| Visual Studio / Rider / VS Code | Qualquer versão recente |

---

## Variáveis e Configuração

Crie o arquivo `src/EBL.FIG.Process.Identity.Api/appsettings.Development.json` com o conteúdo abaixo (nunca commitar credenciais reais):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Initial Catalog=BDSDIDENTITY;User ID=sa;Password=SUA_SENHA;MultipleActiveResultSets=True;Max Pool Size=200;TrustServerCertificate=True;Connection Timeout=30;"
  },
  "Swagger": {
    "Enabled": true
  },
  "HangfireDashboard": {
    "Enabled": true,
    "RequireBasicAuth": false,
    "Username": "admin",
    "Password": "dev-only"
  },
  "Database": {
    "AutoMigrate": true
  },
  "Security": {
    "JwtMasterKey": "CHAVE-ALEATORIA-UUID-AQUI"
  },
  "JwtKeyManagement": {
    "EncryptionKey": "MESMA-CHAVE-ACIMA"
  },
  "JwtSettings": {
    "Issuer": "EBL.FIG.Process.Identity.Api",
    "Audience": "EBL.FIG.Process.Identity.Api",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30,
    "RequireHttpsMetadata": false,
    "ClockSkewMinutes": 1
  },
  "RateLimiting": {
    "EnableRateLimiting": false,
    "GeneralRules": {
      "PermitLimit": 1000,
      "Window": "00:01:00",
      "QueueLimit": 100
    },
    "AuthenticationEndpoints": {
      "PermitLimit": 100,
      "Window": "00:01:00",
      "QueueLimit": 20
    }
  },
  "Cors": {
    "EnableCors": true,
    "PolicyName": "IdentityCorsPolicy",
    "AllowedOrigins": [ "*" ],
    "AllowedMethods": [ "*" ],
    "AllowedHeaders": [ "*" ],
    "AllowCredentials": false,
    "MaxAge": 86400
  }
}
```

### Descrição das variáveis

| Chave | Descrição |
|---|---|
| `ConnectionStrings:DefaultConnection` | String de conexão principal (tabelas da aplicação) |
| `Database:AutoMigrate` | `true` aplica migrations pendentes automaticamente na inicialização |
| `Security:JwtMasterKey` | Chave mestra usada para **descriptografar** a chave RSA privada de cada tenant armazenada no banco |
| `JwtKeyManagement:EncryptionKey` | Mesma chave usada na **criptografia** da chave RSA privada ao gerar um novo par de chaves |
| `JwtSettings:Issuer` | Valor do claim `iss` do JWT |
| `JwtSettings:Audience` | Valor do claim `aud` do JWT |
| `JwtSettings:AccessTokenExpirationMinutes` | Validade do Access Token em minutos |
| `JwtSettings:RefreshTokenExpirationDays` | Validade do Refresh Token em dias |
| `JwtSettings:RequireHttpsMetadata` | `false` em desenvolvimento; `true` em produção |
| `JwtSettings:ClockSkewMinutes` | Tolerância de diferença de relógio entre servidores (em minutos) |
| `Swagger:Enabled` | `true` habilita o Swagger UI — **nunca definir `true` em Production** |
| `HangfireDashboard:Enabled` | `true` habilita o dashboard — **nunca definir `true` em Production** |
| `HangfireDashboard:RequireBasicAuth` | Se `true`, exige usuário/senha para acessar `/hangfire` |
| `HangfireDashboard:Password` | Senha do Basic Auth — valores `changeme` e `dev-only` são rejeitados em Production |
| `RateLimiting:EnableRateLimiting` | Ativa ou desativa o rate limiting globalmente |

---

## Banco de Dados

### Criação via migrations (recomendado para desenvolvimento)

Com `Database:AutoMigrate: true`, a API aplica as migrations automaticamente ao iniciar.

Para aplicar manualmente via CLI:

```powershell
cd src\EBL.FIG.Process.Identity.Api

dotnet ef database update `
  --project ..\EBL.FIG.Process.Identity.Infra.Data\EBL.FIG.Process.Identity.Infra.Data.csproj `
  --startup-project .
```

Para criar uma nova migration:

```powershell
dotnet ef migrations add NomeDaMigration `
  --project ..\EBL.FIG.Process.Identity.Infra.Data\EBL.FIG.Process.Identity.Infra.Data.csproj `
  --startup-project .
```

### Criação via script SQL

O script completo de criação de tabelas está em `docs/sql/Create-Tables.sql`. Ele é executado automaticamente na inicialização se o arquivo existir. Para executar manualmente no SQL Server Management Studio:

```sql
USE BDSDIDENTITY;
GO
-- cole o conteúdo de docs/sql/Create-Tables.sql aqui
```

### Modelo de dados (resumo)

```
Tenants ──┬── Apps
          ├── Users ──── UserRoles ──── Roles ──── RolePermissions ──┬── Resources
          │                                                           └── Actions
          ├── JwtKeys
          └── RefreshTokens
```

| Tabela | Descrição |
|---|---|
| `Tenants` | Clientes/organizações isoladas no sistema |
| `Apps` | Aplicações registradas por tenant |
| `Users` | Usuários por tenant com hash de senha PBKDF2 |
| `Roles` | Papéis por tenant e aplicação |
| `Resources` | Recursos (ex: "Users", "Tenants") por app |
| `Actions` | Ações (ex: "Read", "Create", "Delete") por app |
| `RolePermissions` | Mapeamento Role → Resource + Action |
| `UserRoles` | Mapeamento User → Role + App |
| `JwtKeys` | Par de chaves RSA por tenant (privada criptografada com AES) |
| `RefreshTokens` | Refresh tokens com suporte a revogação e rotação |

---

## Seed de Dados

O seed é executado automaticamente na inicialização junto com as migrations. Para implementar dados de seed, edite `src/EBL.FIG.Process.Identity.Infra.Data/Seeders/DatabaseSeeder.cs`.

### Sequência mínima para funcionar (SQL manual)

Execute na ordem abaixo após criar as tabelas:

```sql
-- 1. Tenant da própria aplicação Identity
INSERT INTO dbo.Tenants (Name, Description, Alias, IsActive, IsDeleted, AddedBy)
VALUES ('Identity', 'Tenant da aplicação Identity', 'identity', 1, 0, 1);
-- Id gerado: 1

-- 2. App dentro do tenant
INSERT INTO dbo.Apps (TenantId, Name, Description, IsActive, IsDeleted, AddedBy)
VALUES (1, 'Identity API', 'API de autenticação e autorização', 1, 0, 1);
-- Id gerado: 1

-- 3. Actions básicas
INSERT INTO dbo.Actions (TenantId, AppId, Name, Description, IsActive, IsDeleted, AddedBy)
VALUES
  (1, 1, 'Read',     'Leitura',           1, 0, 1),
  (1, 1, 'Create',   'Criação',           1, 0, 1),
  (1, 1, 'Update',   'Edição',            1, 0, 1),
  (1, 1, 'Delete',   'Exclusão',          1, 0, 1),
  (1, 1, 'GetActive','Obter ativo',       1, 0, 1),
  (1, 1, 'Revoke',   'Revogar',           1, 0, 1);

-- 4. Resources básicos
INSERT INTO dbo.Resources (TenantId, AppId, Name, Description, IsActive, IsDeleted, AddedBy)
VALUES
  (1, 1, 'Tenants',  'Gestão de tenants',   1, 0, 1),
  (1, 1, 'Users',    'Gestão de usuários',  1, 0, 1),
  (1, 1, 'Roles',    'Gestão de papéis',    1, 0, 1),
  (1, 1, 'JwtKeys',  'Chaves JWT',          1, 0, 1),
  (1, 1, 'Apps',     'Gestão de apps',      1, 0, 1);

-- 5. Role Admin
INSERT INTO dbo.Roles (TenantId, AppId, Name, Description, IsActive, IsDeleted, AddedBy)
VALUES (1, 1, 'Admin', 'Administrador total', 1, 0, 1);
-- Id gerado: 1

-- 6. Criar o primeiro usuário via endpoint (ver seção Execução Local)
-- Após registrar, anotar o UserId retornado e executar:

-- 7. Atribuir role Admin ao usuário (substituir <userId> pelo Id retornado no registro)
INSERT INTO dbo.UserRoles (TenantId, AppId, UserId, RoleId, IsActive, IsDeleted, AddedBy)
VALUES (1, 1, <userId>, 1, 1, 0, 1);
```

> **Atenção**: senhas nunca são armazenadas em texto plano. Use sempre `POST /v1/auth/register` para criar usuários.

---

## Execução Local

```powershell
# 1. Restaurar dependências
dotnet restore

# 2. Build
dotnet build

# 3. Executar a API
cd src\EBL.FIG.Process.Identity.Api
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`
- Hangfire Dashboard: `https://localhost:5001/hangfire`

### Primeira execução — checklist

1. Inicie a API — ela cria o banco e aplica as migrations automaticamente.
2. Execute o seed SQL da seção anterior (passos 1 a 5).
3. Crie a chave JWT RSA inicial para o tenant:
   ```
   POST /v1/admin/jwtkeys/1/create-initial
   ```
4. Registre o primeiro usuário:
   ```
   POST /v1/auth/register  { "tenantId": 1, "name": "admin", "secret": "SuaSenha@123" }
   ```
5. Execute o passo 7 do seed SQL para vincular a role Admin ao usuário criado.
6. Faça login e use o `accessToken` nas demais chamadas.

---

## Autenticação

### Fluxo

```
1. POST /v1/auth/register    → cria usuário
2. POST /v1/auth/login       → retorna accessToken + refreshToken
3. [usar accessToken] → Authorization: Bearer {accessToken}
4. POST /v1/auth/refresh     → rotaciona refreshToken, retorna novos tokens
```

### JWT

- Algoritmo: **RS256** (RSA 2048 bits + SHA-256)
- Chave: par RSA **por tenant** — privada criptografada com AES no banco
- Claims no payload:

| Claim | Descrição |
|---|---|
| `sub` | ID do usuário |
| `name` | Nome do usuário |
| `tenantId` | ID do tenant |
| `appId` | ID da aplicação |
| `role` | Nome(s) da(s) role(s) |
| `permissions` | Objeto JSON: `{ "users": ["read","create"], "tenants": ["read"] }` |
| `jti` | UUID único do token (para auditoria) |
| `nbf` / `exp` | Emissão e expiração |

### Refresh Token

- Gerado com **64 bytes aleatórios** via `RandomNumberGenerator` (CSPRNG).
- Armazenado como **hash SHA-256** no banco — o valor nunca persiste em texto.
- **Rotação automática**: cada `POST /refresh` revoga o token atual e emite um novo par.
- Expiração configurável via `JwtSettings:RefreshTokenExpirationDays`.

---

## Endpoints

### Auth — público (sem JWT)

| Método | Rota | Rate Limit | Descrição |
|---|---|---|---|
| `POST` | `/v1/auth/register` | `authentication` | Registra novo usuário |
| `POST` | `/v1/auth/login` | `authentication` | Autentica e retorna tokens |
| `POST` | `/v1/auth/refresh` | `refreshtoken` | Rotaciona refresh token |

### Tenants — requer JWT

| Método | Rota | Permissão necessária |
|---|---|---|
| `GET` | `/v1/tenants` | Tenants / Read |
| `GET` | `/v1/tenants/{id}` | Tenants / Read |
| `GET` | `/v1/tenants/paged` | Tenants / Read |
| `POST` | `/v1/tenants` | Tenants / Create |
| `PUT` | `/v1/tenants/{id}` | Tenants / Update |
| `DELETE` | `/v1/tenants/{id}` | Tenants / Delete |

### JWT Keys — requer JWT (Admin)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/v1/admin/jwtkeys/{tenantId}` | Lista chaves do tenant |
| `GET` | `/v1/admin/jwtkeys/{tenantId}/active` | Retorna chave ativa |
| `POST` | `/v1/admin/jwtkeys/{tenantId}/create-initial` | Cria par RSA inicial (público) |
| `PATCH` | `/v1/admin/jwtkeys/{id}/revoke` | Revoga uma chave |

> Os demais grupos (Users, Roles, Apps, Resources, Actions, RolePermissions, UserRoles, Jobs) seguem o mesmo padrão REST com autorização por recurso/ação via claims do JWT.

---

## Exemplos de Chamadas

### 1. Criar chave JWT RSA inicial para o tenant

```http
POST /v1/admin/jwtkeys/1/create-initial
```

**Resposta** `201 Created`

---

### 2. Registrar usuário

```http
POST /v1/auth/register
Content-Type: application/json

{
  "tenantId": 1,
  "name": "João Silva",
  "secret": "MinhaSenh@123"
}
```

**Resposta** `200 OK`
```json
{
  "userId": 2,
  "userName": "João Silva",
  "tenantId": 1,
  "roleId": 0,
  "roleName": null
}
```

---

### 3. Login

```http
POST /v1/auth/login
Content-Type: application/json

{
  "loginIdentifier": "João Silva",
  "password": "MinhaSenh@123"
}
```

**Resposta** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEifQ...",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoVG9rZW4...",
  "accessTokenExpiresAt": "2025-01-01T01:00:00Z",
  "refreshTokenExpiresAt": "2025-01-31T00:00:00Z",
  "tenantId": 1,
  "tenantName": "Identity",
  "appId": 1,
  "appName": "Identity API",
  "userId": 2,
  "userName": "João Silva",
  "roleId": 1,
  "roleName": "admin"
}
```

---

### 4. Chamada autenticada

```http
GET /v1/tenants
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjEifQ...
```

---

### 5. Renovar tokens (Refresh)

```http
POST /v1/auth/refresh
Content-Type: application/json

{
  "tenantId": 1,
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoVG9rZW4..."
}
```

**Resposta** `200 OK` — retorna novo `accessToken` e novo `refreshToken`. O token anterior é **revogado imediatamente**.

---

### 6. Criar tenant

```http
POST /v1/tenants
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Empresa ABC",
  "description": "Tenant da Empresa ABC",
  "alias": "abc",
  "urlImage": null,
  "settings": null,
  "remarks": null
}
```

**Resposta** `201 Created`

---

### 7. Revogar chave JWT de um tenant

```http
PATCH /v1/admin/jwtkeys/1/revoke
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "reason": "Rotação programada de chaves"
}
```

**Resposta** `200 OK`

---

### Códigos de resposta padrão

| Código | Significado |
|---|---|
| `200` | Sucesso com corpo |
| `201` | Criado com sucesso |
| `204` | Sucesso sem corpo |
| `400` | Requisição inválida (validação de entrada) |
| `401` | Não autenticado / credenciais inválidas |
| `403` | Autenticado, sem permissão para o recurso |
| `404` | Recurso não encontrado |
| `409` | Conflito (ex: nome duplicado) |
| `410` | Recurso removido permanentemente |
| `429` | Rate limit atingido |
| `500` | Erro interno do servidor |

---

## Estratégia Multi-Tenant

A API implementa isolamento de dados por tenant utilizando **Row-Level Security (RLS)** no SQL Server, combinado com o mecanismo de `SESSION_CONTEXT`. Cada request opera exclusivamente sobre os dados do tenant correspondente, sem nenhuma lógica de filtro espalhada pelos repositórios.

### Conceitos Fundamentais

| Conceito | Descrição |
|---|---|
| **Tenant** | Empresa/organização cadastrada. Cada entidade de domínio pertence a um `TenantId`. |
| **RLS (Row-Level Security)** | Política aplicada no SQL Server que filtra automaticamente os dados com base no `SESSION_CONTEXT('TenantId')`. |
| **SESSION_CONTEXT** | Variável de sessão do SQL Server populada antes de cada operação de banco, garantindo que o RLS saiba qual tenant está ativo. |
| **IRequestTenantContext** | Interface de domínio que carrega o `TenantId` resolvido para o request atual (lifetime `Scoped`). |

### Resolução do TenantId

A resolução segue uma ordem de prioridade aplicada tanto no `TenantSessionConnectionInterceptor` quanto no `TenantSessionCommandInterceptor`:

```
1. Usuário autenticado
   └─ Claim 'tenant_id' (ou 'tenantId' / 'tenant') extraída do JWT

2. Usuário não autenticado (ex: login, register)
   └─ IRequestTenantContext populado pelo AppService a partir do body da requisição

3. Nenhuma das anteriores
   └─ SESSION_CONTEXT não é definido → RLS bloqueia o acesso ao banco
```

> **Importante:** A aplicação **nunca** concede acesso super-admin via `SESSION_CONTEXT`. O `TenantId` passado é sempre o do tenant autenticado ou informado explicitamente no payload.

### Fluxo de um Request Autenticado

```
HTTP Request
    │
    ├─ Middleware JWT → valida token, popula HttpContext.User
    │
    ├─ Controller → Application Service
    │
    └─ EF Core executa query
           │
           ├─ TenantSessionConnectionInterceptor (ConnectionOpened)
           │     └─ Lê claim 'tenant_id' do JWT
           │     └─ Executa: sp_set_session_context @key='TenantId', @value=<id>
           │
           ├─ TenantSessionCommandInterceptor (ReaderExecuting / NonQueryExecuting / ScalarExecuting)
           │     └─ Garante SESSION_CONTEXT correto por comando (pool de conexões)
           │
           └─ SQL Server aplica RLS → retorna apenas dados do tenant
```

### Fluxo de um Request Não Autenticado (ex: Login)

```
HTTP Request (POST /auth/login, body: { tenantId, username, password })
    │
    ├─ AuthAppService → chama IRequestTenantContext.SetTenantId(body.TenantId)
    │
    └─ EF Core executa query
           │
           ├─ TenantSessionConnectionInterceptor
           │     └─ HttpContext.User não autenticado
           │     └─ Lê IRequestTenantContext.TenantId
           │     └─ Executa: sp_set_session_context @key='TenantId', @value=<id>
           │
           └─ SQL Server aplica RLS
```

---

## Interceptors

Os interceptors são implementações do EF Core (`DbConnectionInterceptor` / `DbCommandInterceptor`) registrados como `Scoped` no container de DI e adicionados ao `DbContext` via `AddInterceptors()` no `Program.cs`.

### TenantSessionConnectionInterceptor

**Arquivo:** `src/EBL.FIG.Process.Identity.Infra.Data/Interceptors/TenantSessionConnectionInterceptor.cs`

Estende `DbConnectionInterceptor`. Intercepta o evento `ConnectionOpenedAsync` e popula o `SESSION_CONTEXT` do SQL Server com o `TenantId` resolvido no momento em que a conexão é aberta.

**Quando é executado:** Uma vez por abertura de conexão.

**Limitação coberta pelo próximo interceptor:** Conexões reutilizadas do pool podem carregar o `SESSION_CONTEXT` de um request anterior — por isso o `TenantSessionCommandInterceptor` complementa a estratégia.

---

### TenantSessionCommandInterceptor

**Arquivo:** `src/EBL.FIG.Process.Identity.Infra.Data/Interceptors/TenantSessionCommandInterceptor.cs`

Estende `DbCommandInterceptor`. Intercepta todos os comandos SQL (`ReaderExecutingAsync`, `ScalarExecutingAsync`, `NonQueryExecutingAsync`) e garante que o `SESSION_CONTEXT` esteja correto **antes de cada execução**, independentemente do reaproveitamento de conexão pelo pool.

**Quando é executado:** Antes de cada comando SQL executado pelo EF Core.

**Proteção contra recursão:** Verifica se o comando já é um `sp_set_session_context` e o ignora, evitando loop infinito.

---

### TelemetryInterceptor

**Arquivo:** `src/EBL.FIG.Process.Identity.Infra.Data/Interceptors/TelemetryInterceptor.cs`

Estende `DbCommandInterceptor`. Adiciona **logging e telemetria** para todas as operações de banco de dados executadas pelo EF Core, tanto síncronas quanto assíncronas (`ReaderExecuting`, `NonQueryExecuting`, `ScalarExecuting` e suas variantes `*Async`).

Utilizado para observabilidade e diagnóstico de performance das queries.

---

### Registro dos Interceptors

Os interceptors são registrados como `Scoped` no `DependencyInjection.cs` e adicionados ao `DbContext` no `Program.cs`:

```csharp
// DependencyInjection.cs
services.AddScoped<TenantSessionConnectionInterceptor>();
services.AddScoped<TenantSessionCommandInterceptor>();
services.AddScoped<TelemetryInterceptor>();

// Program.cs
builder.Services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(
        serviceProvider.GetRequiredService<TenantSessionConnectionInterceptor>(),
        serviceProvider.GetRequiredService<TenantSessionCommandInterceptor>(),
        serviceProvider.GetRequiredService<TelemetryInterceptor>()
    );
});
```

> **Por que `Scoped`?** Para que os interceptors tenham acesso ao `IRequestTenantContext` e ao `IHttpContextAccessor` com o ciclo de vida correto por request HTTP.