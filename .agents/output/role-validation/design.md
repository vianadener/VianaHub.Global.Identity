# Design Técnico - Role Production Validation

## Objetivo

Validar tecnicamente a implementação do recurso `Role` para confirmar que endpoints, serviço de aplicação, domínio, persistência e testes estão coerentes com a arquitetura do projeto.

## Escopo técnico

- API Minimal para `Role`
- Serviço de aplicação `RoleAppService`
- Serviço de domínio `RoleDomainService`
- Entidade `RoleEntity`
- Repositório `RoleDataRepository`
- Mapeamentos EF Core e AutoMapper
- Validações de rota e de domínio
- Testes de API, aplicação, domínio e infraestrutura

## Arquivos principais

- `src/VianaHub.Global.Identity.Api/Endpoints/RoleEndpoint.cs`
- `src/VianaHub.Global.Identity.Application/Services/RoleAppService.cs`
- `src/VianaHub.Global.Identity.Domain/Services/RoleDomainService.cs`
- `src/VianaHub.Global.Identity.Domain/Entities/RoleEntity.cs`
- `src/VianaHub.Global.Identity.Infra.Data/Repository/RoleDataRepository.cs`
- `src/VianaHub.Global.Identity.Infra.Data/Mappings/RoleMapping.cs`
- `tests/VianaHub.Global.Identity.Tests/Api/RoleEndpointTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Application/RoleAppServiceTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Domain/RoleDomainServiceTests.cs`

## Fluxos a validar

1. Listagem paginada de roles
2. Consulta por id
3. Criação
4. Atualização
5. Ativação e desativação
6. Exclusão
7. Upload em massa
8. Resposta HTTP e contratos do endpoint

## Pontos de arquitetura

- Os endpoints devem permanecer finos e sem regra de domínio.
- O serviço de aplicação deve coordenar o fluxo e preservar o tenant do usuário autenticado.
- O serviço de domínio deve concentrar as regras de negócio do aggregate.
- O repositório deve continuar como única porta de persistência do aggregate.
- O mapeamento EF deve permanecer explícito.

## Riscos observados

- Possível divergência entre autorização declarada e autorização efetiva no endpoint.
- Possível regressão no bulk upload se o tenant ou permissões forem inferidos de origem incorreta.
- Possível inconsistência entre códigos HTTP documentados e retornados.
- Possível duplicidade de regras entre API, aplicação e domínio.

## Critério técnico de pronto

- Os testes cobrindo `Role` passam.
- Não há quebra de compatibilidade dos endpoints existentes.
- A autorização e o isolamento por tenant estão preservados.
- A validação de produção pode seguir para revisão final sem suposições pendentes.
