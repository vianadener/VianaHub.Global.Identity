# QA Output

## Cenários de teste
- listar apps com sucesso
- buscar app por id existente
- buscar app por id inexistente
- criar app com sucesso
- impedir criação duplicada
- atualizar app com sucesso
- ativar e desativar app
- excluir app
- bulk upload com arquivo válido
- bulk upload com arquivo inválido
- bulk upload com arquivo vazio
- validação de autorização por recurso `Apps`

## Dados de teste
- tenant válido
- usuário autenticado com permissões de `Apps`
- app existente
- app inexistente
- CSV válido com colunas esperadas
- CSV com conteúdo inválido

## Cobertura esperada
- unit tests para `AppAppService`
- API tests para `AppEndpoint`
- cobertura de retorno `410` para recurso ausente
- cobertura de autorização por operação

## Resultado Esperado
- comportamento estável nos fluxos principais
- metadata e comportamento da API alinhados
- bulk upload sem dependência indevida de dados externos ao contexto autenticado

## Observações
- a suíte deve garantir que os cenários de ausência de recurso usem o código `410`
- a validação deve preservar a compatibilidade dos endpoints existentes

