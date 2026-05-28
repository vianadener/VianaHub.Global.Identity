# Relatório de Análise — Recurso Tenant

**Data:** 28/05/2026
**Projeto:** VianaHub.Global.Identity
**Analista:** opencode (reviewer agent)

---

## Resumo Executivo

A implementação do multi-tenant segue **DDD + Clean Architecture** com 7 projetos (Api, Application, Domain, Infra.Data, Infra.IoC, Infra.Job, Tests). O isolamento é feito via **RLS (Row-Level Security) + SESSION_CONTEXT** com dois interceptors EF Core (`TenantSessionConnectionInterceptor` e `TenantSessionCommandInterceptor`). Resolução de `TenantId` em 3 prioridades: claim JWT → `IRequestTenantContext` → RLS bloqueia. Jobs Hangfire (`ReconcileJwtKeysJob`, `JwtKeyRotationJob`) mantêm chaves JWT reconciliadas e rotacionadas por tenant.

---

## Pontos Fortes

1. **Arquitetura RLS robusta** — Dois interceptors (Connection + Command) garantem SESSION_CONTEXT correto contra reutilização de pool (`TenantSessionConnectionInterceptor.cs`, `TenantSessionCommandInterceptor.cs`)

2. **Resolução de TenantId em 3 camadas** — JWT → `IRequestTenantContext` → RLS, suportando autenticados, não-autenticados e background jobs sem HttpContext (`TenantSessionConnectionInterceptor.cs:60-107`)

3. **Defesa contra recursão infinita** — `TenantSessionCommandInterceptor` ignora comandos `sp_set_session_context` duplicados (`TenantSessionCommandInterceptor.cs:76-79`)

4. **Tratamento de transação no Command Interceptor** — `SetTenantSessionContextAsync` associa transação ativa ao comando SQL (`TenantSessionCommandInterceptor.cs:162-163`)

5. **Resolução chicken-and-egg no JWT** — `JwtSetup.cs` decodifica sem validar assinatura para extrair `tenant_id` antes de buscar chaves JWT (`JwtSetup.cs:101-137`)

6. **Validação em dois níveis** — API validators + Domain validators com FluentValidation e suporte a localização

7. **Bulk upload com sanitização** — Validação CSV, normalização UTF-8, limitação de linhas, validação de duplicidade (`TenantAppService.cs:136-275`)

8. **Jobs com resiliência por tenant** — Try/finally isolando falhas por tenant e continuando para os próximos (`ReconcileJwtKeysJob.cs:57-72`, `JwtKeyRotationJob.cs:52-86`)

9. **Testes abrangentes** — Cobertura em todas as camadas: interceptors, repository, app service, endpoint, validators, e jobs

10. **Endpoints RESTful** — Versionados (`/v1/tenants`), autorização por RBAC (`CustomAuthorize`), códigos HTTP corretos (201, 204, 404, 410)

---

## Issues Encontrados

### Issue 1: IsSuperAdmin bypass contradiz documentação dos interceptors
- **Severidade:** Crítico
- **Descrição:** `TenantDataRepository.GetByUserNameAsync` e `GetByLoginIdentifierAsync` executam `sp_set_session_context @key=N'IsSuperAdmin', @value=1` manualmente, abrindo e fechando a conexão. Isso contradiz a documentação do `TenantSessionCommandInterceptor` que afirma: *"A aplicação NUNCA seta IsSuperAdmin no SESSION_CONTEXT"* (`TenantSessionCommandInterceptor.cs:20`). Além disso, abre a conexão manualmente, bypassando o interceptor de conexão.
- **Localização:** `TenantDataRepository.cs:26-36`, `TenantDataRepository.cs:41-56`
- **Sugestão:** Criar uma política RLS específica para operações de lookup cross-tenant (login) sem depender de `IsSuperAdmin`. Alternativas: (1) Stored procedure de lookup com permissão elevada; (2) Política RLS com predicate `WHERE TenantId = SESSION_CONTEXT(N'TenantId') OR SESSION_CONTEXT(N'IsSuperAdmin') = 1`; (3) Connection dedicada sem RLS.

### Issue 2: GetTenantId retorna valor default de 1 em produção
- **Severidade:** Crítico
- **Descrição:** `CurrentUserApiService.GetTenantId()` retorna `1` quando não encontra claim nem header, com TODO indicando que é para desenvolvimento. Em produção, requisições sem token válido acessariam sempre o Tenant 1.
- **Localização:** `CurrentUserApiService.cs:91-93`
- **Sugestão:** Lançar exceção ou retornar 0 quando não encontrar tenant, em vez do default `1`. O mesmo vale para `GetAppId()` na linha 136.

### Issue 3: RequestTenantContext não é thread-safe para async
- **Severidade:** Alto
- **Descrição:** `RequestTenantContext` usa um campo simples `private int? _tenantId`. Em cenários com async/await, continuations podem executar em threads diferentes, causando race conditions. O padrão correto para request-scoped é usar `AsyncLocal<T>`.
- **Localização:** `RequestTenantContext.cs:7`
- **Sugestão:** Usar `AsyncLocal<int?>`:
  ```csharp
  private readonly AsyncLocal<int?> _tenantId = new();
  public int? TenantId => _tenantId.Value;
  ```

### Issue 4: Entity.Id com setter público quebra encapsulamento DDD
- **Severidade:** Alto
- **Descrição:** A classe base `Entity` expõe `public int Id { get; set; }` com setter público. Em DDD, o Id deve ser protegido para evitar alterações acidentais.
- **Localização:** `Entity.cs:10`
- **Sugestão:** Alterar para `public int Id { get; protected set; }` ou `public int Id { get; private set; }`.

### Issue 5: TenantId no AuthAppService propagado mas nunca utilizado
- **Severidade:** Médio
- **Descrição:** `AuthAppService` declara `private int TenantId { get; set; }` como propriedade setada no construtor. A propriedade nunca é usada — cada método define `_requestTenantContext.SetTenantId()` explicitamente.
- **Localização:** `AuthAppService.cs:33, 63`
- **Sugestão:** Remover a propriedade `TenantId` e a injeção desnecessária no construtor.

### Issue 6: Validação duplicada entre Api Validators e Domain Validators
- **Severidade:** Médio
- **Descrição:** `CreateTenantRouteValidator` (Api) e `CreateTenantValidator` (Domain) validam os mesmos campos com as mesmas regras. Viola DRY.
- **Localização:** `CreateTenantRouteValidator.cs:15-33` vs `CreateTenantValidator.cs:11-27`
- **Sugestão:** Manter validação apenas no Domain (authoritative) e usar formatação básica no endpoint. Ou criar shared validator reutilizável.

### Issue 7: Delete/Activate/Deactivate chamam UpdateAsync genérico
- **Severidade:** Baixo
- **Descrição:** `TenantDomainService` usa `_repo.UpdateAsync()` para DeleteAsync, ActivateAsync e DeactivateAsync. Funcionalmente correto (soft delete), mas nome enganoso.
- **Localização:** `TenantDomainService.cs:87, 101, 115`
- **Sugestão:** Criar método `UpdateStateAsync` ou documentar que `UpdateAsync` é genérico.

### Issue 8: Queries cross-aggregate no repositório de Tenant
- **Severidade:** Médio
- **Descrição:** `ITenantDataRepository` contém `GetByUserNameAsync` e `GetByLoginIdentifierAsync` que fazem Include de Users no TenantEntity. Viola princípio de que repositório de aggregate não deveria navegar para outro aggregate.
- **Localização:** `ITenantDataRepository.cs:10-11`, `TenantDataRepository.cs:24-56`
- **Sugestão:** Mover para service de lookup cross-aggregate ou criar método específico no repositório de Users.

### Issue 9: Bulk upload sem transação atômica
- **Severidade:** Médio
- **Descrição:** `ProcessBulkItemsAsync` itera items e cria cada tenant individualmente. Se o 50º de 100 items falhar, os 49 anteriores já foram commitados.
- **Localização:** `TenantAppService.cs:228-264`
- **Sugestão:** Documentar que bulk upload é "best-effort" e retorna erros por item. Ou usar transação explícita com rollback.

### Issue 10: LIKE sem escape de caracteres especiais
- **Severidade:** Baixo
- **Descrição:** `GetPagedAsync` usa `EF.Functions.Like` sem escapar `%`, `_`, `[` no search. Causa resultados inesperados.
- **Localização:** `TenantDataRepository.cs:86-89`
- **Sugestão:** Implementar helper `EscapeLikePattern()`.

### Issue 11: Typo no método ResolvetenantId
- **Severidade:** Baixo
- **Descrição:** Inconsistência de casing no método `ResolvetenantId()` (camelCase com 't' minúsculo).
- **Localização:** `TenantSessionConnectionInterceptor.cs:60`
- **Sugestão:** Renomear para `ResolveTenantId()`.

### Issue 12: Testes de interceptors não testam SqlConnection real
- **Severidade:** Alto
- **Descrição:** Testes usam `FakeDbConnection`, verificando apenas o caminho "ignorar quando não é SqlConnection". Nenhum teste valida definição real do SESSION_CONTEXT.
- **Localização:** `TenantSessionConnectionInterceptorTests.cs`, `TenantSessionCommandInterceptorTests.cs`
- **Sugestão:** Criar testes de integração com SQL Server real (Testcontainers).

### Issue 13: ReconcileJwtKeysJob faz GetAllAsync() sem setar SESSION_CONTEXT
- **Severidade:** Alto
- **Descrição:** `ReconcileJwtKeysJob.Execute()` chama `_tenantRepo.GetAllAsync()` sem setar SESSION_CONTEXT antes. Com RLS ativo, pode retornar apenas tenants do contexto atual.
- **Localização:** `ReconcileJwtKeysJob.cs:51`
- **Sugestão:** Usar `IsSuperAdmin` ou connection dedicada para obter lista completa de tenants.

### Issue 14: Falta paginação no GetAllAsync de jobs
- **Severidade:** Médio
- **Descrição:** Jobs carregam todos os tenants na memória via `GetAllAsync()`. Para muitos tenants, pode causar problemas de memória.
- **Localização:** `ReconcileJwtKeysJob.cs:51`, `JwtKeyRotationJob.cs:46`
- **Sugestão:** Usar `GetPagedAsync` ou `IAsyncEnumerable` para processar em lotes.

---

## Recomendações Priorizadas

| Prioridade | Ação |
|------------|------|
| **Máxima** | Resolver bypass `IsSuperAdmin` e fallback `TenantId=1` — riscos de segurança |
| **Máxima** | Converter `RequestTenantContext` para `AsyncLocal<T>` |
| **Alta** | Refatorar queries cross-aggregate para service dedicado |
| **Alta** | Adicionar testes de integração com SQL Server real (Testcontainers) |
| **Alta** | Proteger `Entity.Id` com `protected set` |
| **Média** | Consolidar validação em uma única camada |
| **Média** | Documentar comportamento "best-effort" do BulkUpload |
| **Média** | Implementar `EscapeLikePattern()` para search |

---

## Resumo de Severidade

| Severidade | Quantidade |
|------------|------------|
| Crítico | 2 |
| Alto | 4 |
| Médio | 5 |
| Baixo | 3 |
| **Total** | **14** |
