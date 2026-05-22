# Security Output

## Findings
- os endpoints do recurso `App` aplicam autorização por papel, recurso e ação
- o serviço preserva isolamento por tenant via `ICurrentUserService`
- o bulk upload não deve aceitar dados que escapem do contexto autenticado

## Impacto
- baixo

## Recomendações
- manter a consistência entre `Produces(...)` e o código real retornado pelo serviço
- manter validação de arquivo e sanitização do CSV
- continuar validando `TenantId` e usuário autenticado em todas as operações sensíveis

## Status
- aprovado

