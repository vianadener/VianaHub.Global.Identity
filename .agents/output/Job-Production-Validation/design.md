# Feature: Job Production Validation

## Contexto
O recurso `Job` já está implementado e precisa de validação de produção para confirmar estabilidade funcional, consistência de contratos, autorização, e integração com agendamento/Hangfire.

## Decisões de Arquitetura
- Validar a implementação existente sem reescrever o recurso.
- Corrigir apenas inconsistências comprovadas entre comportamento real e metadata da API.
- Manter compatibilidade com os endpoints já expostos.

## Estrutura por Camada
- Domain: regras de entidade e validação de `JobDefinitionEntity`
- Application: orquestração, validações e integração com scheduler
- Infra.Data: persistência de job definitions e mappings
- Infra.Job: execução, agendamento e sincronização com Hangfire
- Api: endpoints, autorização e metadata Swagger
- Tests: regressão funcional, agendamento e autorização

## Entidades e Agregados
- `JobDefinitionEntity` como entidade principal do recurso
- `JobContext` como contexto operacional para execução/sincronização

## Interfaces e Serviços
- `IJobAppService`
- `IJobDefinitionDataRepository`
- `IJobSchedulerService`
- `IJobExecutor`
- `IJobSyncService`

## Persistência e Integração
- validar que o estado da entidade é persistido corretamente após ativação, desativação, atualização e exclusão
- validar integração com Hangfire ao registrar ou remover jobs recorrentes
- validar que a execução manual usa o scheduler corretamente

## Estratégia de Testes
- testes unitários para `JobAppService`
- testes de API para `JobEndpoint`
- validação de códigos HTTP retornados e documentados
- validação de criação, atualização, ativação, desativação, exclusão e execução
- validação de sincronização/agendamento de jobs

## Riscos e Trade-offs
- risco de divergência entre metadata do endpoint e comportamento real
- risco de regressão ao alterar integração com scheduler/Hangfire
- risco de falha de validação em regras de cron, prioridade e estado ativo
- trade-off: manter a correção mínima para preservar backward compatibility

## Inconsistências a Validar
- `GetById`, `Execute`, `Update`, `Activate`, `Deactivate` e `Delete` retornam `410` quando o recurso não existe
- a documentação do endpoint deve refletir o comportamento real
- a execução só deve ocorrer quando o job estiver ativo
- o repositório e o scheduler devem permanecer sincronizados após mudanças de estado

