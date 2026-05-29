# AGENTS.md — VianaHub.Global.Identity

## Project Overview

.NET 8 multi-tenant Identity API with JWT RS256, RBAC, and Refresh Token rotation.

## Mandatory Directives

All agents **must** follow these guidelines at all times:

- **Language**: Always respond in Portuguese (Brazil)
- **Architecture**: Hexagonal Architecture (Ports & Adapters)
- **Principles**: SOLID
- **Design**: DDD (Domain-Driven Design)
- **Structure**: Clean Architecture (layers: Domain → Application → Infra → Api)
- **i18n**: Multi-language support via `Localization/**/*.json`

## Quick Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet test --settings .runsettings  # with code coverage (coverlet, opencover)
```

## Solution Structure

| Project | Responsibility |
|---------|---------------|
| `VianaHub.Global.Identity.Api` | Entry point, Minimal API endpoints, middleware, config |
| `VianaHub.Global.Identity.Application` | Use cases, DTOs, app service interfaces |
| `VianaHub.Global.Identity.Domain` | Entities, Value Objects, domain interfaces, validators |
| `VianaHub.Global.Identity.Infra.Data` | EF Core, repositories, interceptors, seeders, context |
| `VianaHub.Global.Identity.Infra.IoC` | All DI registration in `DependencyInjection.cs` |
| `VianaHub.Global.Identity.Infra.Integration` | External integrations (email sender no-op) |
| `VianaHub.Global.Identity.Infra.Job` | Hangfire jobs, hosted services |
| `VianaHub.Global.Identity.Tests` | Unit/integration tests |

## Key Architecture Facts

- **Multi-tenant isolation**: Row-Level Security (RLS) + `SESSION_CONTEXT`. Two interceptors (`TenantSessionConnectionInterceptor`, `TenantSessionCommandInterceptor`) set TenantId per request. No super-admin bypass.
- **DI centralized**: All registrations in `src/VianaHub.Global.Identity.Infra.IoC/DependencyInjection.cs`
- **Endpoints**: Mapped via `[EndpointMapper]` attribute + `MapEndpointsFromAssembly()` in Program.cs
- **Validators**: FluentValidation with localization (`Localization/**/*.json`)
- **EF Mappings**: Explicit — avoid implicit cascades
- **Never put domain logic in endpoints**

## Conventions

- **Language**: Code and comments in English. Communication in Portuguese (Brazil)
- **Branches**: `feature/history-XXXX`, `feature/issue-XXXX`, `fix/issue-XXXX` from `develop`
- **Never commit without explicit user authorization**
- **Preserve backward compatibility of endpoints**
- **Run `dotnet build` and `dotnet test` before finishing any task**

## Dependencies

- .NET SDK 8.0
- SQL Server 2019+ (Express works)
- `appsettings.Development.json` must exist at `src/VianaHub.Global.Identity.Api/` (see README for template)

## Testing

- Framework: xUnit + Moq + NBuilder + EF InMemory
- Coverage: coverlet, configured in `.runsettings`
- Run single test: `dotnet test --filter "FullyQualifiedName~ClassName"`

## GitHub Projects

**Board:** `https://github.com/users/vianahub-pt/projects/1`
**Repo:** `vianahub-pt/VianaHub.Global.Identity`

### Project IDs (for `gh` commands)

| Field | ID |
|-------|-----|
| Project ID | `PVT_kwHODGRT384BZCnv` |
| Status Field ID | `PVTSSF_lAHODGRT384BZCnvzhUEIlE` |

### Status Option IDs (Kanban Columns)

| Coluna | Option ID | Responsável | Quando card vai para cá | Ação |
|--------|-----------|-------------|------------------------|------|
| **Backlog** | `f75ad846` | PO | Card criado | Cria issue no GitHub, documenta |
| **To do** | `eda9b53c` | PO → Developer | Card pronto para dev | Developer cria branch e implementa |
| **In Progress** | `47fc9ee4` | Developer | Developer pega o card | Implementa, testa, faz commit |
| **For Tests** | `a42b88c6` | Developer → QA | Developer termina | Passa para QA validar |
| **In Test** | `94a9d6f6` | QA | QA pega o card | Testa, valida, gera relatório |
| **For Deploy** | `add10e44` | QA → DevOps | QA aprova | Passa para deploy |
| **Done** | `98236657` | DevOps | Deploy completo | Card finalizado |

### Kanban Flow Rules

1. **Each card has an owner** — only the responsible agent moves the card
2. **No skipping steps** — cards must flow in order: Backlog → To do → In Progress → For Tests → In Test → For Deploy → Done
3. **PO creates and prioritizes** — creates issue in Backlog, moves to To do when ready
4. **Developer implements** — moves to In Progress, then to For Tests when done
5. **QA validates** — moves to In Test, then to For Deploy when approved
6. **DevOps deploys** — moves to Done when deployed

### Environment Variable

Agents must have `GH_TOKEN` set to interact with GitHub Projects:
```powershell
$env:GH_TOKEN = "ghp_..."  # classic token with 'project' scope
```

## Subagent Orchestration

The workflow follows a strict chain through Kanban columns. **AUTOMATION IS MANDATORY** — agents must automatically invoke the next agent in the chain without human intervention.

```
PO → DEVELOPER → QA → DevOps → Human (PR Approval Only)
Backlog → To do → In Progress → For Tests → In Test → For Deploy → Done
```

### Automated Agent Chain

| Agent | Columns | Action | Automation |
|-------|---------|--------|------------|
| **PO** | Backlog → To do | Creates issue, documents, moves to To do | **AUTOMATICALLY invokes DEVELOPER** |
| **DEVELOPER** | To do → In Progress → For Tests | Implements, tests, commits, creates PR | **AUTOMATICALLY invokes QA** |
| **QA** | For Tests → In Test → For Deploy | Validates, tests, generates report | **AUTOMATICALLY invokes DevOps** |
| **DevOps** | For Deploy → Done | Deploys to production | **AUTOMATICALLY moves to Done** |
| **Human** | For Deploy → Done | **ONLY approves PR** (merge feature → develop) | **MANUAL — Never done by agents** |

### Automation Rules (CRITICAL)

1. **PO → DEVELOPER**: When PO finishes creating issues and moving to "To do", the PO **MUST** automatically call the DEVELOPER agent using the Task tool. No human intervention required.

2. **DEVELOPER → QA**: When DEVELOPER finishes implementing and moves to "For Tests", the DEVELOPER **MUST** automatically call the QA agent using the Task tool. No human intervention required.

3. **QA → DevOps**: When QA finishes validating and moves to "For Deploy", the QA **MUST** automatically call the DevOps agent using the Task tool. No human intervention required.

4. **Human PR Approval**: The **ONLY** human intervention is approving the Pull Request (merge from feature branch to develop). Agents **NEVER** approve PRs.

5. **No Skipping**: Cards must flow in order: Backlog → To do → In Progress → For Tests → In Test → For Deploy → Done

6. **No Human Intervention in Agent Flow**: Humans do NOT invoke agents. Agents invoke each other automatically.

### PR Approval Workflow

```
DEVELOPER creates PR (feature/issue-XXX → develop)
    ↓
Human reviews and APPROVES the PR (merge)
    ↓
DevOps deploys to production
    ↓
Card moves to Done
```

**Rules:**
- Agents create PRs but **NEVER** approve or merge them
- Only humans can approve PRs (merge from feature to develop)
- After PR is approved, DevOps agent deploys and moves card to Done
- All issues must live in GitHub Projects
