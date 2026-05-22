# Codex CLI Runbook

Este runbook define como executar os agentes do projeto `VianaHub.Global.Identity` com o Codex CLI.

## Objetivo

Padronizar a execução de ponta a ponta de uma feature, desde a solicitação inicial até o merge em `develop`.

## Pré-requisitos

- Repositório atualizado localmente
- Prompts dos agentes disponíveis em:
  - `.agents/po/system-prompt.md`
  - `.agents/tech-lead/system-prompt.md`
  - `.agents/qa/system-prompt.md`
  - `.agents/developer/system-prompt.md`
  - `.agents/security/system-prompt.md`
- Fluxo documentado em `.agents/AGENTS-README.md`
- Catálogo de comandos em `.agents/CODEX-CLI-COMMANDS.md`
- Template de sessão em `.agents/FEATURE-SESSION-TEMPLATE.md`

## Ordem de Execução

1. PO
2. Tech Lead
3. QA
4. Developer
5. Security
6. Review final
7. Commit e push da branch
8. PR para `develop`
9. Merge

## Encadeamento Automático

- Ao finalizar, o PO deve disparar o Tech Lead sem aguardar confirmação humana.
- Ao finalizar, o Tech Lead deve disparar QA e Developer.
- Ao finalizar, o QA deve registrar a cobertura e liberar a continuação imediata do Developer quando aplicável.
- Ao finalizar, o Developer deve executar commit, push e disparar o Security.
- Ao finalizar, o Security deve disparar a revisão final e a abertura do PR quando não houver bloqueios.
- O fluxo só deve parar para interação humana em caso de impedimento, decisão de escopo ou aprovação externa.

## Intervenções Humanas Permitidas

- Aprovar ou rejeitar uma mudança quando a política do repositório exigir revisão humana.
- Interromper o fluxo para corrigir bloqueios de segurança.
- Ajustar escopo quando o requisito original mudar.
- Confirmar merge somente quando a automação do repositório não puder concluir sozinha.

## Entradas e Saídas por Agente

### PO
- Entrada: solicitação inicial do usuário
- Saída: feature em Gherkin e critérios de aceite

### Tech Lead
- Entrada: feature do PO
- Saída: design técnico, impactos por camada e decisões arquiteturais

### QA
- Entrada: feature do PO + design do Tech Lead
- Saída: cenários de teste, fixtures e cobertura esperada

### Developer
- Entrada: feature do PO + design do Tech Lead + cenários do QA
- Saída: implementação, testes e branch preparada

### Security
- Entrada: feature, design e implementação
- Saída: findings, riscos, recomendações e status de segurança

## Padrão de Execução no Codex CLI

Use o Codex CLI para carregar cada agente com o respectivo prompt e executar a tarefa correspondente.

Exemplo de fluxo:

```text
1. Carregar o prompt do PO
2. Executar a análise da feature
3. Persistir a saída
4. Carregar o prompt do Tech Lead
5. Executar o design técnico
6. Persistir a saída
7. Carregar os prompts de QA e Developer
8. Executar QA e implementação conforme dependências
9. Carregar o prompt de Security
10. Executar a revisão de segurança
11. Consolidar review final
12. Fazer commit e push da branch
13. Abrir PR
14. Fazer merge em develop após aprovação
```

## Regra de Orquestração

O Codex CLI deve tratar a saída de cada agente como entrada automática do próximo agente, preservando os artefatos intermediários e reduzindo a intervenção humana ao mínimo necessário.

## Convenção de Artefatos

- Feature: `{NomeDaFeature}.feature`
- Requisitos adicionais do PO: `{NomeDaFeature}-Requisitos.md`
- Design técnico: `{NomeDaFeature}-Design.md`
- Testes: `{NomeDaFeature}.Tests.cs` ou estrutura equivalente do projeto
- Relatório de segurança: `{NomeDaFeature}-Security.md`

## Checklist de Controle

- [ ] Feature clara e rastreável
- [ ] Design técnico aprovado
- [ ] Testes definidos e executados
- [ ] Implementação concluída
- [ ] Revisão de segurança concluída
- [ ] PR aberto para `develop`
- [ ] Merge aprovado

## Observação

Se um novo tipo de agente for adicionado, este runbook deve ser atualizado antes do uso em produção do fluxo.
