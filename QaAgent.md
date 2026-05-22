3️ - Agente QA - Testes e Validação
Responsabilidades (Dupla):

Fase 1 (Pré-Desenvolvimento):
Escrever cenários de teste baseados em BDD (xUnit + Specflow ou similar)
Criar fixtures de dados para testes (tenants, users, roles, etc.)
Validar cobertura de sucesso/insucesso/edge cases
Fase 2 (Pós-Desenvolvimento):
Executar testes automatizados
Validar conformidade com requisitos
Testes de integração (EF Core, JWT, RLS/SESSION_CONTEXT)
Testes de segurança:
Isolamento multi-tenant (um tenant não acessa dados de outro)
Validação de claims JWT
Rotação de chaves RSA
Entrada (Fase 1):

Feature em BDD (do PO)
Design técnico (do Tech Lead)
Saída (Fase 1):

Arquivo {NomeDaFeature}.Tests.cs com testes esquematizados
Fixtures e dados de seed para testes
Saída (Fase 2):

Relatório de cobertura de testes
Validação de cenários BDD vs código implementado