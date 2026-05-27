# Análise Completa — VianaHub.Global.Identity

> Data: 27/05/2026
> Versão do código: Conforme branch atual

---

## 1. Visão Geral

O **VianaHub.Global.Identity** é um serviço de identidade e autenticação (SSO/IGA) multi-tenant com:

- Gestão de usuários, roles, permissões, tenants, aplicações, recursos e ações
- JWT com rotação de chaves RSA
- Refresh tokens e reset de senha
- Row-Level Security (RLS) no SQL Server
- Hangfire para jobs agendados (rotação de chaves, sincronização)
- Internacionalização (pt-PT, en-US, es-ES)
- OpenTelemetry para observabilidade

### Estrutura do Projeto

| Camada | Projeto | Propósito |
|--------|---------|-----------|
| Presentation | `VianaHub.Global.Identity.Api` | Minimal API, middleware, swagger, i18n |
| Application | `VianaHub.Global.Identity.Application` | App Services, DTOs, AutoMapper |
| Domain | `VianaHub.Global.Identity.Domain` | Entidades, Value Objects, Domain Services, Validators |
| Infrastructure | `VianaHub.Global.Identity.Infra.Data` | EF Core, Repositories, Interceptors, RLS |
| Infrastructure | `VianaHub.Global.Identity.Infra.IoC` | DI centralizado |
| Infrastructure | `VianaHub.Global.Identity.Infra.Integration` | Email (placeholder) |
| Infrastructure | `VianaHub.Global.Identity.Infra.Job` | Hangfire jobs, Hosted Services |
| Tests | `VianaHub.Global.Identity.Tests` | xUnit + Moq + Coverlet |

### Stack Tecnológica

- .NET 8
- EF Core 9.0 + SQL Server
- Minimal API
- FluentValidation
- AutoMapper
- Serilog
- Hangfire
- Swagger / Swashbuckle
- Row-Level Security (SQL Server)
- OpenTelemetry

---

## 2. Pontos Fortes

### 2.1 Arquitetura

| Item | Descrição |
|------|-----------|
| Clean Architecture + DDD | 7 projetos em 4 camadas com separação clara de responsabilidades |
| DI centralizada | `DependencyInjection.cs` registra todos os serviços em um único local |
| Domain Services ricos | Lógica de negócio não vaza para endpoints, com validação via `FluentValidation` |
| RLS Multi-tenant nativo | Interceptors + security policies no SQL Server — abordagem madura |

### 2.2 Segurança

| Item | Descrição |
|------|-----------|
| JWT com rotação de chaves RSA | `JwtKeyRotationJob` + `JwtKeyManagementService` para rotação automática |
| Refresh tokens | Armazenados em banco com suporte a revogação |
| Password reset com token | Tabela `PasswordResetTokens` com expiração |
| Proteção multi-tenant via RLS | Block predicates para INSERT/UPDATE/DELETE |
| Políticas de autorização | `BackOffice` policy com `RequireAuthenticatedUser()` |
| Rate limiting | Configurável por política |

### 2.3 Infraestrutura

| Item | Descrição |
|------|-----------|
| Middleware pipeline robusto | 6 middlewares: localization, exception handling, domain exception, notification, HTTP protocol, JSON exception |
| Health checks | Endpoint com verificação de SQL Server |
| Serilog | Logging estruturado com rolling file e console |
| Hangfire | Jobs com filas dinâmicas (`UseHangfireServerWithDynamicQueues`) |
| OpenTelemetry | `TelemetryInterceptor` + `DatabaseTelemetry` |

### 2.4 Código e Testes

| Item | Descrição |
|------|-----------|
| Minimal API limpa | `MapEndpointsFromAssembly()` para descoberta automática de endpoints |
| Validação robusta | 10 validadores de entidade via FluentValidation |
| Swagger customizado | Tema dark, seletor de idioma (pt-PT, en-US, es-ES) |
| Testes configurados | xUnit + Moq + Coverlet + Microsoft.AspNetCore.Mvc.Testing |

### 2.5 Documentação

| Item | Descrição |
|------|-----------|
| README completo | 648 linhas com visão geral, arquitetura, setup, endpoints, exemplos |
| Scripts SQL | Create-Tables, Initial seed, remoção |
| ISSUE docs | 5 documentos com escopo funcional e critérios de aceite |
| Agentes AI | Skills, runbooks, templates para PO, Tech Lead, QA, Developer, Security |

---

## 3. Melhorias Necessárias

### 3.1 🔴 CRÍTICO — Segurança

| # | Problema | Localização | Impacto |
|---|----------|-------------|---------|
| 1 | `appsettings.Development.json` com credenciais reais no repositório | `src/VianaHub.Global.Identity.Api/appsettings.Development.json` | IP real do servidor (82.29.172.68), usuários/senhas de banco, JWT master key e chave de criptografia expostos |
| 2 | Chave AES hardcoded no código fonte | `Domain/Tools/Cryptography/CryptoAES.cs:7` | `2B7E151628AED2A6ABF7158809CF4F3C` fixa no código |
| 3 | `JwtMasterKey` = `EncryptionKey` (mesmo UUID) | `appsettings.Development.json` | Reduz a segurança da criptografia das chaves privadas RSA |
| 4 | `appsettings.json` com senha padrão "changeme" para Hangfire | `src/.../Api/appsettings.json:42` | Produção sem garantia de override via env vars |

**Ações recomendadas:**

1. Rotacionar **imediatamente** todas as credenciais expostas
2. Adicionar `appsettings.Development.json` ao `.gitignore`
3. Migrar segredos para Azure Key Vault / AWS Secrets Manager / dotnet User Secrets
4. Extrair chave AES para configuração segura
5. Usar chaves distintas para JWT master e criptografia RSA

### 3.2 🔴 CRÍTICO — Arquitetura

| # | Problema | Localização |
|---|----------|-------------|
| 5 | `Application.csproj` referencia `Infra.Data` e `Infra.Integration` | `src/.../Application/*.csproj` linhas 18-20 |
| 6 | `AuthAppService` injeta `IdentityDbContext` diretamente | `Application/Services/AuthAppService.cs:29,45` |
| 7 | `Domain.csproj` depende de `Hosting.Abstractions` | `src/.../Domain/*.csproj:15` |

**Análise:**

- **Problema 5**: Viola Clean Architecture — a camada de Application deve depender apenas de Domain. As referências para Infra.Data e Infra.Integration quebram o fluxo de dependência.
- **Problema 6**: `AuthAppService` usa `IdentityDbContext` diretamente (incluindo `Commit()` e `Rollback()`). Isso acopla a aplicação ao EF Core. Deveria usar repositórios e um `IUnitOfWork`.
- **Problema 7**: Domínio não deve depender de abstrações de infraestrutura/hosting.

### 3.3 🔴 ALTA — Funcionalidades Incompletas

| # | Problema | Localização | Impacto |
|---|----------|-------------|---------|
| 8 | `DatabaseSeeder.SeedAsync()` vazio | `Infra.Data/Seeders/DatabaseSeeder.cs` | Nenhum dado inicial é populado na primeira execução |
| 9 | `NoOpEmailSender` apenas faz log | `Infra.Integration/Email/NoOpEmailSender.cs` | Reset de senha e notificações não funcionam |
| 10 | Pastas `Interfaces/`, `Services/`, `Tools/` vazias em `Infra.Integration` | Projeto sem implementação real de integração |

### 3.4 🟡 MÉDIA — Qualidade de Código

| # | Problema | Localização | Detalhes |
|---|----------|-------------|----------|
| 11 | Typo `tanantId` (21 ocorrências) | `Domain/Services/RoleDomainService.cs`, `ActionDomainService.cs` + interfaces | `tanantId` → `tenantId` |
| 12 | `public set` inconsistente | `ResourceEntity.cs:13`, `ActionEntity.cs:13` | Demais entidades usam `private set` |
| 13 | ActivitySource com nome errado | `Infra.Data/Telemetry/DatabaseTelemetry.cs:14,20` | Usa `"VianaHub.Global.Gerit.Database"` (outro projeto) |
| 14 | CryptoMD5 e dead code | `Domain/Tools/Cryptography/CryptoMD5.cs` | Classe completa sem uso em produção |
| 15 | `TestAutoMapperSetup.cs` na raiz de `src/` | Artefato de exploração em local inadequado |

### 3.5 🟡 MÉDIA — DevOps / Infraestrutura

| # | Problema | Localização | Detalhes |
|---|----------|-------------|----------|
| 16 | Nenhum CI/CD pipeline | `.github/workflows/` contém apenas `opencode.yml` | Sem build, test ou deploy automatizado |
| 17 | Nenhum Dockerfile | Projeto sem containerização | Dependência de SQL Server externo sem provisionamento |
| 18 | `.runsettings` com `net6.0` | `.runsettings:5` | Projeto é .NET 8 — cobertura pode falhar |
| 19 | Versões inconsistentes de pacotes | Múltiplos `.csproj` | `Microsoft.Extensions.*` em versões 8.0, 9.0, 9.0.8 e 10.0.0 |
| 20 | Swagger e Hangfire com `Enabled: false` mas config de auth ativa | Inconsistência de configuração |

### 3.6 🟢 BAIXA — Melhorias

| # | Problema | Localização |
|---|----------|-------------|
| 21 | Pastas de teste vazias (`Caching/`, `Integration/`, `Messaging/`) | `tests/.../Infra/` |
| 22 | `Nullable` disabled nos testes | `Tests.csproj` — inconsistente com src (enabled) |
| 23 | `launchSettings.json` sem profile de produção | `Properties/launchSettings.json` |
| 24 | Nomeaçao inconsistente no README | README referencia `EBL.FIG.Process.Identity.*` |

---

## 4. Roadmap Recomendado

### Fase 1 — Segurança (imediata)

| Ordem | Ação | Esforço |
|-------|------|---------|
| 1 | Rotacionar credenciais expostas (DB, JWT, criptografia) | 1h |
| 2 | Adicionar `appsettings.Development.json` ao `.gitignore` | 5min |
| 3 | Migrar secrets para Azure Key Vault / User Secrets | 4h |
| 4 | Extrair chave AES do código para configuração segura | 1h |
| 5 | Separar `JwtMasterKey` de `EncryptionKey` | 1h |

### Fase 2 — Arquitetura (curto prazo)

| Ordem | Ação | Esforço |
|-------|------|---------|
| 6 | Remover dependências de Infrastructure do `Application.csproj` | 2h |
| 7 | Refatorar `AuthAppService` para usar repositórios + `IUnitOfWork` | 4h |
| 8 | Remover dependência de `Hosting.Abstractions` do Domain | 1h |
| 9 | Corrigir `tanantId` → `tenantId` (21 ocorrências) | 30min |

### Fase 3 — Funcionalidades (médio prazo)

| Ordem | Ação | Esforço |
|-------|------|---------|
| 10 | Implementar `DatabaseSeeder.SeedAsync()` | 4h |
| 11 | Implementar `IEmailSender` real (SendGrid/SMTP/AWS SES) | 8h |
| 12 | Preencher `Infra.Integration` com interfaces e serviços | 4h |

### Fase 4 — DevOps (médio prazo)

| Ordem | Ação | Esforço |
|-------|------|---------|
| 13 | Criar GitHub Actions para build + test + coverage | 4h |
| 14 | Dockerizar a aplicação (Dockerfile + docker-compose) | 4h |
| 15 | Centralizar versões de pacotes (`Directory.Build.props`) | 1h |
| 16 | Corrigir `.runsettings` para net8.0 | 5min |
| 17 | Configurar Hangfire com senha via env var obrigatória | 1h |

### Fase 5 — Qualidade (longo prazo)

| Ordem | Ação | Esforço |
|-------|------|---------|
| 18 | Aumentar cobertura de testes (especialmente integração) | 16h+ |
| 19 | Remover dead code (`CryptoMD5`, `DomainExtensions.Encrypt`) | 1h |
| 20 | Corrigir ActivitySource name | 30min |
| 21 | Unificar encapsulamento das entities (`private set`) | 30min |
| 22 | Remover `TestAutoMapperSetup.cs` da raiz de src/ | 5min |

---

## 5. Resumo de Riscos

| Risco | Severidade | Probabilidade | Mitigação |
|-------|-----------|---------------|-----------|
| Exposição de credenciais reais no repositório | 🔴 Crítica | Alta | Rotacionar imediatamente + .gitignore + Key Vault |
| Violação de Clean Architecture impede escalabilidade | 🟡 Alta | Média | Refatorar acoplamentos no curto prazo |
| Reset de senha quebrado (NoOpEmailSender) | 🟡 Alta | Alta | Implementar provedor de email real |
| Conflitos de runtime por versões inconsistentes | 🟡 Média | Média | Centralizar versões com Directory.Build.props |
| Cobertura de testes insuficiente para produção | 🟡 Média | Média | Aumentar cobertura gradualmente |

---

## 6. Conclusão

O **VianaHub.Global.Identity** é um projeto maduro, bem estruturado e com arquitetura sólida (DDD + Clean Architecture + RLS). A documentação é exemplar e o código segue boas práticas.

Os principais riscos estão na **exposição de credenciais** (que deve ser tratada imediatamente) e nas **violações de Clean Architecture na camada Application** (que comprometem a manutenibilidade a longo prazo). As funcionalidades de email e seed de banco estão incompletas e precisam de implementação.

Com as correções propostas, o projeto está apto para produção com segurança e qualidade.
