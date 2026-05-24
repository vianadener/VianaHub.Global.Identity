# ISSUE: Validação da implementação do recurso Resource

Resumo
------
Validar a implementação do recurso `Resource` para confirmar consistência entre endpoints, serviço de aplicação, autorização, persistência e testes automatizados antes de considerar a entrega pronta para produção.

Objetivo
--------
- Verificar o comportamento funcional do recurso `Resource`.
- Confirmar que a autorização está aplicada corretamente por operação.
- Garantir que a documentação e a implementação estejam consistentes.
- Validar que os códigos HTTP expostos seguem o padrão permitido pelo projeto.
- Identificar regressões nos fluxos de criação, consulta, paginação, atualização, ativação, desativação, exclusão e upload em massa.

Arquivos principais para validação
----------------------------------
- `src/VianaHub.Global.Identity.Api/Endpoints/ResourceEndpoint.cs`
- `src/VianaHub.Global.Identity.Application/Services/ResourceAppService.cs`
- `src/VianaHub.Global.Identity.Domain/Entities/ResourceEntity.cs`
- `src/VianaHub.Global.Identity.Domain/Services/ResourceDomainService.cs`
- `tests/VianaHub.Global.Identity.Tests/Api/ResourceEndpointTests.cs`
- `tests/VianaHub.Global.Identity.Tests/Application/ResourceAppServiceTests.cs`

Escopo funcional
----------------
1. Listagem de resources
2. Consulta por id
3. Paginação
4. Criação
5. Atualização
6. Ativação
7. Desativação
8. Exclusão
9. Bulk upload

Pontos de validação
-------------------
- Os endpoints devem manter compatibilidade com o contrato existente.
- A autorização por papel, recurso e ação deve permanecer coerente com o restante da API.
- O serviço de aplicação deve respeitar o contexto de tenant e usuário autenticado.
- A camada de API não deve concentrar regra de domínio.
- Os testes devem cobrir sucesso, falha, ausência de recurso e o fluxo de upload em massa.

Critérios de aceite
-------------------
- Todos os fluxos principais do recurso `Resource` passam em testes.
- Não há divergência entre códigos HTTP documentados e os retornados pela aplicação.
- Não há uso de códigos HTTP fora do padrão permitido pelo projeto.
- Não há regressões de autorização ou isolamento por tenant.
- A feature está validada e pronta para revisão final.

Observações
-----------
- A validação deve preservar backward compatibility.
- Se houver divergência entre metadata do endpoint e comportamento real, o problema deve ser tratado como falha de implementação.
- A feature aqui descrita deve ser usada como base para a execução dos agentes de PO, Tech Lead, QA, Developer e Security.

Fim do relatório.
