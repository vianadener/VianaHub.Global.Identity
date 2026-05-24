# Security - Resource Production Validation

## Objetivo

Validar riscos de segurança do recurso `Resource` antes de considerar a entrega apta para produção.

## Áreas de verificação

- Autorização por endpoint e por operação
- Isolamento por tenant
- Escalada de privilégio
- Exposição indevida de dados
- Validação de payload e upload em massa
- Riscos comuns de OWASP

## Riscos a revisar

- Usuário sem permissão acessando operações sensíveis
- Usuário de tenant diferente manipulando dados fora do escopo
- Bulk upload permitindo criação/alteração fora do contexto autenticado
- Respostas com dados além do necessário
- Falhas de validação permitindo payload malformado

## Pontos de atenção

- Confirmar que a autorização é aplicada no servidor e não apenas no cliente.
- Confirmar que o tenant ativo vem do contexto autenticado.
- Confirmar que as rotas sensíveis não aceitam bypass por claims manipuladas.
- Confirmar que logs e respostas não expõem segredos, tokens ou dados internos sensíveis.

## Resultado esperado

- Sem vulnerabilidade crítica identificada
- Sem bypass de autorização
- Sem quebra de isolamento por tenant
- Sem exposição desnecessária de dados
