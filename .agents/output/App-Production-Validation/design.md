# Feature: App Production Validation

## Contexto
O recurso `App` já está implementado e precisa de validação de produção para confirmar estabilidade funcional, consistência de contratos e alinhamento entre API, aplicação e testes.

## Decisões de Arquitetura
- Validar a implementação existente sem reescrever o recurso.
- Corrigir apenas inconsistências comprovadas entre comportamento real e metadata da API.
- Manter compatibilidade com os endpoints já expostos.

## Estrutura por Camada
- Domain: regras de entidade e agregados de `App`
- Application: orquestração, contexto do usuário e bulk upload
- Infra.Data: persistência e filtros por tenant
- Api: endpoints, autorização e metadata Swagger
- Tests: regressão funcional e validação de autorização

## Entidades e Agregados
- `AppEntity` como entidade principal do recurso
- `TenantId` como escopo de isolamento

## Interfaces e Serviços
- `IAppAppService`
- `IAppDataRepository`
- `IAppDomainService`
- `ICurrentUserService`

## Persistência e Mapeamento
- validar que a filtragem por tenant permanece consistente
- validar que bulk upload não aceita dados que violem o contexto autenticado

## Estratégia de Testes
- testes unitários para `AppAppService`
- testes de API para `AppEndpoint`
- validação de códigos HTTP retornados e documentados
- validação de bulk upload com CSV válido e inválido
- validação de permissões por operação

## Riscos e Trade-offs
- risco de divergência entre metadata do endpoint e comportamento real
- risco de regressão em bulk upload se o contexto do usuário não for usado corretamente
- trade-off: manter a correção mínima para preservar backward compatibility

## Inconsistências a Validar
- `GetById`, `Update`, `Activate`, `Deactivate` e `Delete` retornam `410` no serviço quando o recurso não existe
- a documentação do endpoint deve refletir o comportamento real
- o bulk upload deve respeitar o contexto autenticado e não depender de dados fora do escopo do usuário

