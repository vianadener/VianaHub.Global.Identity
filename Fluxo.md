Fluxo de Interação entre Agentes

┌─────────────────────────────────────────────────────────────────┐
│ FEATURE SOLICITADA PELO USUÁRIO                                 │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
           ┌──────────────────────┐
           │  PO: BDD Feature     │
           │  - Gherkin scenarios │
           │  - Success/Failure   │
           └──────────┬───────────┘
                      │
                      ▼
      ┌────────────────────────────────────┐
      │ Tech Lead: Design & Refinement     │
      │ - Architecture validation (Hex)    │
      │ - DDD/SOLID compliance            │
      │ - Multi-tenancy analysis          │
      │ - Migration planning              │
      │ - Security considerations         │
      └──────────┬─────────────────────────┘
                 │
        ┌────────┴────────┐
        │                 │
        ▼                 ▼
   ┌─────────────┐   ┌──────────────┐
   │ QA: Test    │   │ Developer:   │
   │ Plan        │   │ Implementation
   │ - Fixtures  │   │ (Paralelo)   │
   │ - Test code │   └──────────────┘
   └─────────────┘         │
                           ▼
                    ┌──────────────────┐
                    │ Dev: Code in     │
                    │ feature branch   │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ QA: Execution    │
                    │ & Validation     │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ Security: Code   │
                    │ Review & Scan    │
                    └────────┬─────────┘
                             │
                    ┌────────┴────────┐
                    │                 │
                   YES               NO ──→ Feedback ao Dev
                    │                      (Retorna para refactor)
                    ▼
        ┌──────────────────────────┐
        │ PR: Merge feature branch │
        │ para develop             │
        │ (Code Review + CI/CD)    │
        └──────────┬───────────────┘
                   │
                   ▼
        ┌──────────────────────────┐
        │ ✅ Feature pronta em     │
        │ develop branch           │
        │ (Pronta para produção)   │
        └──────────────────────────┘