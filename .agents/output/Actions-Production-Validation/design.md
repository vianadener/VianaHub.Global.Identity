# Feature: Actions Production Validation

## Contexto
O recurso `Actions` já está implementado e precisava de uma validação real de produção para detectar bugs funcionais e riscos de segurança.

## Decisões de Arquitetura
- Validar a implementação atual sem reescrever o recurso.
- Corrigir apenas os pontos que comprometeram integridade, autorização ou consistência.
- Manter compatibilidade com os endpoints existentes.

## Estrutura por Camada
- Domain: validações e regras de entidade
- Application: orquestração, contexto do usuário e bulk upload
- Infra.Data: persistência e filtros por tenant/app
- Api: endpoints e autorização
- Tests: validação de regressão no escopo de Actions

## Riscos Identificados
- bulk upload usando `AppId` enviado no CSV em vez do contexto do usuário
- autorização do bulk upload apontando para recurso incorreto
- cobertura de segurança não explicitando esse fluxo

## Ação Tomada
- corrigir o uso de `AppId` no bulk upload para o contexto autenticado
- corrigir o recurso da autorização do bulk upload para `Actions`
- alinhar os testes da API com a permissão correta
