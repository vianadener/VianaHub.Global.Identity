# Codex CLI Commands

Este documento descreve modelos práticos de execução para cada agente.

## Uso

Carregue o prompt do agente correspondente e execute a tarefa da etapa atual da feature.

## PO

Entrada:
- solicitação inicial da feature

Saída:
- feature em Gherkin
- requisitos adicionais, se necessários

Modelo:

```text
Executar PO com a solicitação inicial da feature.
Salvar a feature em Gherkin e os requisitos em artefatos separados.
```

## Tech Lead

Entrada:
- feature do PO

Saída:
- design técnico

Modelo:

```text
Executar Tech Lead com a feature criada pelo PO.
Produzir o design técnico com impacto por camada, entidades, interfaces e riscos.
```

## QA

Entrada:
- feature do PO
- design do Tech Lead

Saída:
- cenários de teste
- fixtures
- cobertura esperada

Modelo:

```text
Executar QA com a feature e o design técnico.
Listar cenários de teste e cobertura esperada antes da implementação.
```

## Developer

Entrada:
- feature do PO
- design do Tech Lead
- cenários do QA

Saída:
- implementação
- testes
- branch preparada

Modelo:

```text
Executar Developer com a feature, o design e os cenários de teste.
Implementar somente o escopo aprovado.
```

## Security

Entrada:
- feature
- design
- implementação

Saída:
- findings
- recomendações
- status de segurança

Modelo:

```text
Executar Security após a implementação.
Revisar autenticação, autorização, isolamento por tenant e exposição de dados.
```

## Review Final

Entrada:
- feature
- design
- implementação
- testes
- review de segurança

Saída:
- decisão final para PR

Modelo:

```text
Consolidar os resultados dos agentes e validar se a entrega está pronta para PR e merge em develop.
```
