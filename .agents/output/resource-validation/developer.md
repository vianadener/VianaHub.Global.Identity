# Developer - Resource Production Validation

## Objetivo

Consolidar possíveis correções técnicas para o recurso `Resource`, caso a validação encontre divergências.

## Diretrizes

- Priorizar correções pequenas e localizadas
- Evitar quebra de compatibilidade
- Manter a regra de negócio no domínio
- Não duplicar lógica entre endpoint e aplicação

## Possíveis ajustes

- Ajustar autorização por operação se algum endpoint estiver inconsistente
- Corrigir tenant resolution no serviço de aplicação se houver divergência
- Reforçar testes de bulk upload e cenários de regressão
- Ajustar validações de rota e de domínio se houver inconsistência de contrato
- Corrigir mapeamento EF ou AutoMapper se houver divergência entre modelo e persistência

## Entrega esperada

- Lista objetiva de correções
- Escopo mínimo de alteração
- Base para aplicação técnica sem ampliar o requisito
