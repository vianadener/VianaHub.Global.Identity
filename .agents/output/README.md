# Output Directory

Esta pasta armazena os artefatos gerados por execução de feature com o Codex CLI.

## Estrutura esperada

```text
.agents/output/
  <FeatureName>/
    po.feature
    design.md
    qa.md
    developer.md
    security.md
    metadata.md
```

## Template Base

Para iniciar uma nova feature com arquivos prontos, copie a pasta `.agents/output/feature-template` e renomeie para o nome da feature.

## Uso

- Crie uma pasta por feature.
- Grave aqui os artefatos de cada agente.
- Use nomes consistentes com o template de sessão em `.agents/FEATURE-SESSION-TEMPLATE.md`.

## Observação

Se um artefato adicional for necessário, adicione-o dentro da pasta da feature sem alterar o contrato principal acima.
