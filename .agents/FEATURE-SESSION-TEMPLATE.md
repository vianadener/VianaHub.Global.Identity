# Feature Session Template

Use este template para organizar uma execução completa de feature com o Codex CLI.

## Metadados

- Nome da feature: `{{FeatureName}}`
- Branch: `feature/{{FeatureName}}`
- Data de início: `{{StartDate}}`
- Responsável pela execução: `Codex CLI`

## Etapas

### 1. PO
- entrada: solicitação inicial
- saída: `{{FeatureName}}.feature`

### 2. Tech Lead
- entrada: feature do PO
- saída: `{{FeatureName}}-Design.md`

### 3. QA
- entrada: feature + design
- saída: cenários e cobertura

### 4. Developer
- entrada: feature + design + QA
- saída: implementação e testes

### 5. Security
- entrada: implementação completa
- saída: findings e status

## Artefatos

- `.agents/output/{{FeatureName}}/po.feature`
- `.agents/output/{{FeatureName}}/design.md`
- `.agents/output/{{FeatureName}}/qa.md`
- `.agents/output/{{FeatureName}}/developer.md`
- `.agents/output/{{FeatureName}}/security.md`

## Checklist

- [ ] PO concluído
- [ ] Tech Lead concluído
- [ ] QA concluído
- [ ] Developer concluído
- [ ] Security concluído
- [ ] PR aberto
- [ ] Merge em `develop`
