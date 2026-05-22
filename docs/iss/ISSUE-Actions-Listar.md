# ISSUE: Inconsistência de códigos HTTP e uso de código proibido — recurso: apps

Resumo
------
Durante a auditoria do recurso `apps` foram identificadas inconsistências entre as respostas documentadas nos endpoints e o comportamento efetivo da camada de aplicação. Em particular, os endpoints expõem `404 NotFound` nas anotações (`.Produces(...)`), enquanto o serviço de aplicação (`AppAppService`) utiliza o código `410 Gone` quando o recurso não é encontrado. O código `404` NÃO faz parte da lista de códigos HTTP permitidos para o sistema (códigos válidos: 200, 201, 204, 400, 401, 403, 409, 410, 429, 500).

Arquivos afetados
-----------------
- `src/EBL.FIG.Process.Identity.Api/Endpoints/AppEndpoint.cs`
- `src/EBL.FIG.Process.Identity.Application/Services/AppAppService.cs`

Evidências
----------
1) Em `AppEndpoint.cs` os endpoints declaram `.Produces<ErrorResponse>(StatusCodes.Status404NotFound)` nos seguintes handlers:
   - `GET /v1/apps/{id}` (GetById)
   - `PUT /v1/apps/{id}` (Update)
   - `PATCH /v1/apps/{id}/activate` (Activate)
   - `PATCH /v1/apps/{id}/deactivate` (Deactivate)
   - `DELETE /v1/apps/{id}` (Delete)

   Exemplo: `groupV1.MapGet("/{id}", ... ).Produces<AppResponse>(StatusCodes.Status200OK).Produces<ErrorResponse>(StatusCodes.Status404NotFound)`

2) Em `AppAppService.cs` a lógica de negócio verifica a existência da entidade e, quando não encontrada, adiciona uma notificação com código `410` (Gone):
   - `if (entity == null) { _notify.Add(_localization.GetMessage("...ResourceNotFound"), 410); return null/false; }`

Consequências
-------------
- Uso de um código HTTP proibido (`404`) na metadata do endpoint (Swagger) — isto viola a lista de códigos permitidos definida para a auditoria.
- Inconsistência entre documentação/metadata do endpoint (`404`) e o comportamento real do serviço (`410`). Isso pode gerar documentação incorreta (Swagger), confundir consumidores da API e testes automatizados.

Observações
-----------
- As validações de rota e domínio para `apps` (tamanhos e obrigatoriedade de `Name` e `Description`) estão alinhadas com o script SQL `docs/sql/Create-Tables.sql` (Name NVARCHAR(200), Description NVARCHAR(500)) e com os validators presentes no código.
- Autorização está aplicada (RequireAuthorization no group e `CustomAuthorize` por operação); não foram encontradas evidências, no escopo desta auditoria, de falha de autorização para `apps`.

Severidade
---------
- Média: inconsitência de códigos HTTP e uso de código proibido em documentação/metadata do endpoint.

Recomendação (registro do problema)
-----------------------------------
- Alinhar os códigos HTTP declarados em `AppEndpoint.cs` com os códigos que o serviço efetivamente retorna (substituir `404` por `410` onde aplicável) ou alterar a lógica da aplicação para usar `404` se essa for a opção desejada. Em qualquer caso, garantir que apenas os códigos permitidos sejam expostos e que documentação e implementações estejam consistentes.

Fim do relatório.
