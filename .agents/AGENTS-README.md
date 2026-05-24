# Agentes Autônomos para Desenvolvimento de Features

Este repositório usa um fluxo de agentes para transformar uma demanda de negócio em implementação validada, revisada e pronta para merge em `develop`.

O orquestrador é o **Codex CLI**, que executa cada agente a partir do prompt dedicado e mantém o trabalho distribuído entre papéis especializados.

## Objetivo

- Receber uma solicitação de feature em linguagem de negócio.
- Converter a solicitação em feature/história de usuário quando necessário.
- Refinar a solução técnica e o design.
- Implementar, testar e revisar a entrega.
- Validar segurança e aderência arquitetural.
- Levar a implementação até a branch `develop` com rastreabilidade.

## Fluxo Geral

`Solicitação inicial -> PO -> Tech Lead -> QA + Developer -> Security -> Review final -> Merge para develop`

## Automação Entre Agentes

- Cada agente deve produzir sua saída e acionar automaticamente a próxima etapa do fluxo.
- Cada etapa deve começar com uma mensagem explícita no formato `Agente em execução: <NomeDoAgente>`.
- O PO, ao concluir, entrega a feature diretamente ao Tech Lead.
- O Tech Lead, ao concluir, entrega o design técnico ao QA e ao Developer.
- O QA e o Developer devem avançar de forma encadeada conforme dependências, sem aguardar intervenção humana entre tarefas.
- O Developer, ao concluir implementação e testes, faz commit e push e aciona o Security.
- O Security, ao concluir, aciona a revisão final e a etapa de PR/merge.
- A intervenção humana só deve ocorrer em bloqueios, aprovações explícitas ou mudança de escopo.

## Pontos Manuais Remanescentes

- Aprovação de política do repositório, quando exigida para merge.
- Aprovação explícita em bloqueios de segurança ou inconsistências de arquitetura.
- Mudança de escopo solicitada pelo usuário durante a execução.
- Decisão final em caso de conflito entre requisitos, segurança e compatibilidade.

## Papéis dos Agentes

### 1. PO - Product Owner

Responsabilidade:

- Receber a solicitação inicial da feature.
- Verificar se já existe feature/história de usuário para o tema.
- Criar ou atualizar a feature em Gherkin.
- Garantir que o escopo esteja claro antes de delegar para os demais agentes.

Passo a passo:

1. Ler o prompt inicial do usuário.
2. Identificar o objetivo de negócio.
3. Verificar se já existe feature/história relacionada.
4. Se não existir, criar a feature em Gherkin.
5. Se já existir, reutilizar a feature existente e apenas complementar o necessário.
6. Delegar a feature para o Tech Lead.

Saída esperada:

- Arquivo `.feature` em Gherkin.
- Resumo do escopo e dos critérios de aceite.

### 2. Tech Lead

Responsabilidade:

- Refinar o desenho técnico da feature.
- Validar aderência à arquitetura do projeto.
- Quebrar a feature em tarefas técnicas.
- Garantir que o plano técnico seja implementável pelo Developer e verificável pelo QA.

Passo a passo:

1. Receber a feature do PO.
2. Analisar impacto em domínio, aplicação, infraestrutura e API.
3. Validar convenções do projeto.
4. Produzir o documento de design técnico.
5. Definir as tarefas e dependências.
6. Encaminhar o design para QA e Developer.

Saída esperada:

- Arquivo `{NomeDaFeature}-Design.md`.
- Lista objetiva de tarefas técnicas.

### 3. QA

Responsabilidade:

- Derivar casos de teste a partir da feature e do design.
- Cobrir cenários de sucesso, falha e bordas.
- Garantir rastreabilidade entre requisito, design e validação.

Passo a passo:

1. Ler a feature e o design técnico.
2. Identificar os cenários testáveis.
3. Escrever ou atualizar testes automatizados conforme a estratégia do projeto.
4. Validar pré-condições, entradas, saídas e mensagens de erro.
5. Verificar se a implementação atende os critérios de aceite.

Saída esperada:

- Plano de testes ou suíte de testes atualizada.
- Evidência de cobertura dos cenários principais.

### 4. Developer

Responsabilidade:

- Implementar a feature no código.
- Respeitar DDD, Clean Architecture, SOLID, Repository Pattern e Unit of Work.
- Preservar compatibilidade dos endpoints existentes.
- Não concentrar lógica de domínio em endpoints.

Passo a passo:

1. Ler a feature, o design e os testes esperados.
2. Identificar os arquivos e camadas afetadas.
3. Implementar a solução seguindo as convenções do projeto.
4. Ajustar mappings, serviços, validadores e contratos quando necessário.
5. Executar build e testes relevantes.
6. Preparar a branch da feature para revisão.

Saída esperada:

- Código implementado.
- Testes atualizados ou adicionados.
- Branch pronta para PR.

### 5. Security

Responsabilidade:

- Revisar a implementação com foco em segurança.
- Validar impacto em autenticação, autorização, isolamento de dados e exposição de informações.
- Identificar regressões de segurança ou pontos de ataque.

Passo a passo:

1. Ler a feature, o design e a implementação.
2. Verificar superfícies sensíveis como login, autorização, tenant isolation e persistência.
3. Validar se não há exposição indevida de dados.
4. Conferir se regras de segurança e validações estão completas.
5. Registrar ajustes necessários antes do merge.

Saída esperada:

- Lista de achados e recomendações.
- Aprovação ou bloqueios de segurança, quando aplicável.

## Execução Com Codex CLI

O Codex CLI deve executar os agentes em sequência e preservar os artefatos gerados por cada etapa.

Ordem recomendada:

1. Atualizar `develop` com o remoto antes de iniciar qualquer feature.
2. Executar o PO com o prompt da feature.
3. Persistir a saída do PO como artefato da feature.
4. Executar o Tech Lead com a feature criada.
5. Persistir o documento de design técnico.
6. Executar QA e Developer em paralelo quando as dependências permitirem.
7. Consolidar a implementação no branch da feature.
8. Fazer commit das alterações e push da branch para o remoto.
9. Executar Security após a implementação.
10. Fazer review final cruzado entre Tech Lead, QA, Developer e Security.
11. Abrir PR para `develop`.
12. Após aprovação, fazer merge para `develop`.

Para o passo a passo operacional de execução, consulte [CODEX-CLI-RUNBOOK.md](./CODEX-CLI-RUNBOOK.md).
Para comandos por agente e template de sessão, consulte [CODEX-CLI-COMMANDS.md](./CODEX-CLI-COMMANDS.md) e [FEATURE-SESSION-TEMPLATE.md](./FEATURE-SESSION-TEMPLATE.md).

## Regras de Coordenação

- O PO define o problema, não a solução técnica.
- O Tech Lead define a solução técnica, mas não implementa o código final.
- O Developer implementa o que foi especificado, sem ampliar escopo sem alinhamento.
- O QA valida conformidade com a feature e o design.
- O Security valida riscos e exposição.
- Nenhum agente deve assumir o papel de outro sem necessidade explícita.
- Cada agente é responsável por acionar o próximo agente imediatamente após concluir sua etapa.

## Critério de Conclusão

A feature só é considerada concluída quando:

- A feature/história estiver documentada.
- O design técnico estiver validado.
- A implementação estiver concluída.
- Os testes relevantes estiverem passando.
- A revisão de segurança estiver aprovada.
- O PR estiver mesclado em `develop`.

## Observação

Este documento deve servir como guia operacional dos agentes. Se o fluxo mudar, atualize este arquivo antes de alterar o comportamento dos prompts individuais.
