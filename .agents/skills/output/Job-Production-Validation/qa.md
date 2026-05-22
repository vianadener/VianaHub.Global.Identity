# QA Output

## Cenários de teste
- listar jobs com sucesso
- buscar job por id existente
- buscar job por id inexistente
- criar job com sucesso
- impedir criação duplicada
- atualizar job com sucesso
- ativar job e registrar no scheduler quando aplicável
- desativar job e remover do scheduler quando aplicável
- excluir job e remover do scheduler quando aplicável
- executar job ativo com sucesso
- impedir execução de job inexistente
- impedir execução de job inativo
- sincronização/agendamento dos jobs de manutenção

## Dados de teste
- tenant/cenário autenticado válido para a API
- usuário autenticado com permissões de `JobDefinitions`
- job existente
- job inexistente
- job ativo
- job inativo
- request de criação com cron válido
- request de criação com dados inválidos

## Cobertura esperada
- unit tests para `JobAppService`
- API tests para `JobEndpoint`
- cobertura de retorno `410` para recurso ausente
- cobertura de autorização por operação
- cobertura de integração com scheduler/Hangfire

## Resultado Esperado
- comportamento estável nos fluxos principais
- metadata e comportamento da API alinhados
- execução e agendamento sem regressão

## Observações
- a suíte deve garantir que os cenários de ausência de recurso usem o código `410`
- a validação deve preservar a compatibilidade dos endpoints existentes

