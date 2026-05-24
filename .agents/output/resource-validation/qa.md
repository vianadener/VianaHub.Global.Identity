# QA - Resource Production Validation

## Objetivo

Derivar cenários de teste para validar funcionalmente o recurso `Resource` e proteger contra regressões.

## Cenários principais

- Listar resources com sucesso
- Consultar resource por id existente
- Criar resource com payload válido
- Atualizar resource existente
- Ativar resource desativado
- Desativar resource ativo
- Excluir resource existente
- Executar bulk upload com registros válidos

## Cenários de falha

- Consultar resource inexistente
- Criar resource com validação inválida
- Atualizar resource inexistente
- Excluir resource inexistente
- Executar bulk upload com item inválido
- Bloquear acesso sem permissão

## Edge cases

- Paginação sem dados
- Requisições com payload parcial ou inconsistente
- Upload em massa com mistura de itens válidos e inválidos
- Requisições concorrentes sobre o mesmo resource
- Verificação de respostas HTTP fora do contrato esperado

## Cobertura esperada

- `tests/VianaHub.Global.Identity.Tests/Api/ResourceEndpointTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Application/ResourceAppServiceTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Domain/ResourceDomainServiceTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Infra/Data/Repository/ResourceDataRepositoryTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Infra/Data/Mappings/ResourceMappingTests.cs`

## Critério de aceite de QA

- Os fluxos principais estão cobertos.
- As falhas esperadas estão cobertas.
- Não existe regressão visível de contrato, autorização ou tenant isolation.
- O comportamento está pronto para análise de segurança e consolidação do desenvolvimento.
