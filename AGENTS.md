# AGENTS.md

## Idioma
- Responder em Português do Brasil
- Explicações técnicas em português
- Código em inglês
- Comentários de código em inglês

## Stack
- .NET 8, Minimal API, EF Core 9 + SQL Server
- JWT Bearer RS256 (chave RSA por tenant), RBAC stateless
- Hangfire (background jobs com filas dinâmicas)
- AutoMapper, FluentValidation, Serilog, Swashbuckle
- `VianaHub.Global.Middleware` (NuGet externo v1.1.0)

## Arquitetura
- DDD + Clean Architecture + Hexagonal
- 7 projetos: `Api → Application → Domain ← Infra.*`
- DI centralizada em `VianaHub.Global.Identity.Infra.IoC/DependencyInjection.cs`
- Endpoints mapeados via `[EndpointMapper]` + `MapEndpointsFromAssembly()`
- Multi-tenant via RLS + SESSION_CONTEXT com dois interceptors (Connection + Command)
- TelemetryInterceptor para logging de todas as queries
- Domain layer referência `Cronos`, `FluentValidation`, `Microsoft.Extensions.Identity.Core`
- IoC project referência todos os outros infra projects

## Comandos
```powershell
# Build e teste
dotnet build
dotnet test
dotnet test --settings .runsettings              # com cobertura (opencover)

# Migrations (executar de src/VianaHub.Global.Identity.Api)
dotnet ef migrations add Nome --project ..\VianaHub.Global.Identity.Infra.Data --startup-project .
dotnet ef database update --project ..\VianaHub.Global.Identity.Infra.Data --startup-project .

# Executar API
cd src\VianaHub.Global.Identity.Api
dotnet run
# HTTP :5000, HTTPS :5001, Swagger /swagger, Hangfire /hangfire
```

## Convenções
- Não criar múltiplos repositórios por aggregate
- Não colocar lógica de domínio em endpoints
- Preferir Value Objects
- Preservar backward compatibility de endpoints
- EF Mappings explícitos, evitar cascades implícitos
- `CA1822` (mark members as static) silenciado no `.editorconfig`
- `Localization/**/*.json` copiado para output (`CopyToOutputDirectory=PreserveNewest`)
- `appsettings.Development.json` nunca commitar com credenciais reais

## Testes
- xUnit + Moq + NBuilder + EF InMemory
- `using Xunit` global no csproj
- Testes de endpoint usam `Microsoft.AspNetCore.Mvc.Testing`
- Projeto de testes referencia **todos** os projects da solution
- Cobertura via coverlet (formato opencover), configurado em `.runsettings`

## Agentes
- Fluxo completo em `.agents/AGENTS-README.md` com 5 papéis: PO, Tech Lead, QA, Developer, Security
- Orquestrado via Codex CLI (`.github/workflows/opencode.yml`)
- Skills individuais em `.agents/skills/{papel}/SKILL.md`
- Artefatos de feature salvos em `.agents/output/{FeatureName}/`
- Branch: `feature/{FeatureName}`, base: `develop`
- Consultar `.agents/CODEX-CLI-RUNBOOK.md` para runbook operacional

## CI/CD
- Workflow em `.github/workflows/opencode.yml`: executa opencode em comments `/oc` ou `/opencode`
- Modelo: `opencode/gpt-5.4-mini`
