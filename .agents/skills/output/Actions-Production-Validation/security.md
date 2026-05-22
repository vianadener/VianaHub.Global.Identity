# Security Output

## Findings
- bulk upload usava autorização com recurso incorreto (`Plans`) em vez de `Actions`
- bulk upload aceitava `AppId` vindo do CSV, permitindo inconsistência com o contexto autenticado

## Impacto
- médio

## Recomendações
- manter autorização do bulk upload vinculada ao recurso correto
- ignorar `AppId` do arquivo e usar apenas o contexto autenticado

## Status
- aprovado com ressalvas corrigidas
