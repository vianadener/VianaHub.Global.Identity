# EBL.FIG.Process.Identity.API — Documentação Funcional da Base de Dados

## 1. Visão Geral

A base de dados da aplicação **EBL.FIG.Process.Identity.API** foi concebida para suportar um modelo de autenticação, autorização e gestão multi-tenant, permitindo que múltiplas aplicações e organizações utilizem a mesma infraestrutura de identidade de forma isolada e segura.

O sistema fornece:

- Gestão de tenants
- Gestão de aplicações
- Gestão de utilizadores
- Gestão de roles e permissões
- Controlo de autenticação
- Gestão de tokens JWT
- Gestão de refresh tokens
- Gestão de jobs internos do sistema

---

# 2. Conceitos de Negócio

## Tenant

Representa uma organização, cliente ou contexto isolado dentro da plataforma.

Cada tenant possui:

- utilizadores próprios
- aplicações próprias
- roles próprias
- permissões próprias
- chaves JWT próprias

O isolamento lógico é garantido através do campo:

```text
TenantId
```

---

## Aplicação (AppDefinition)

Representa uma aplicação integrada no ecossistema de identidade.

Exemplos:

- Portal Administrativo
- API Financeira
- Aplicação Mobile
- Backoffice Operacional

Cada aplicação:

- pertence a um tenant
- possui contexto próprio
- pode ter hierarquia de aplicações
- possui roles e permissões específicas

---

## Utilizador

Representa uma identidade autenticável dentro de um tenant.

O utilizador pode autenticar-se utilizando:

- email
- username
- outro identificador configurável

O sistema armazena:

- identificador de login
- versão normalizada do login
- hash da password
- estado da conta
- histórico de acessos

---

## Role

Representa um agrupamento funcional de permissões.

Exemplos:

- Administrador
- Gestor
- Supervisor
- Operador
- Auditor

As roles são específicas por:

- tenant
- aplicação

---

## Resource

Representa um recurso protegido da aplicação.

Exemplos:

- Users
- Orders
- Reports
- Invoices
- Configurations

---

## Action

Representa uma ação autorizável sobre um recurso.

Exemplos:

- Create
- Read
- Update
- Delete
- Export
- Approve

---

## Permissão

Uma permissão é composta por:

```text
Role + Resource + Action
```

Exemplo:

```text
Administrador -> Users -> Create
```

---

## JWT Key

Representa a chave criptográfica utilizada para emissão e validação de tokens JWT.

Cada tenant possui:

- apenas uma chave ativa
- política de rotação
- validade temporal
- histórico de utilização

---

## Refresh Token

Representa um token de renovação de sessão autenticada.

Permite:

- renovação de access tokens
- revogação de sessões
- controlo de expiração
- rastreabilidade de acessos

---

## Job

Representa um processo interno automatizado do sistema.

Exemplos:

- limpeza de tokens expirados
- rotação de chaves JWT
- sincronizações
- manutenção
- auditoria

---

# 3. Estrutura Funcional da Base de Dados

## 3.1 Tenants

### Tabela: `Tenants`

Responsável pelo registo das organizações da plataforma.

### Principais responsabilidades

- isolamento lógico da plataforma
- configuração do tenant
- controlo de ativação
- personalização

---

## 3.2 Aplicações

### Tabela: `AppDefinitions`

Responsável pelo registo das aplicações pertencentes a um tenant.

### Principais responsabilidades

- definição de aplicações
- definição de contexto
- hierarquia entre aplicações
- configurações de indexação
- smoke tests

---

## 3.3 Utilizadores

### Tabela: `Users`

Responsável pela gestão das identidades autenticáveis.

### Principais responsabilidades

- autenticação
- controlo de acessos
- gestão de credenciais
- rastreabilidade

---

## 3.4 Roles

### Tabela: `Roles`

Responsável pela definição de perfis funcionais.

---

## 3.5 Recursos

### Tabela: `Resources`

Responsável pela definição dos recursos protegidos.

---

## 3.6 Ações

### Tabela: `Actions`

Responsável pela definição das ações permitidas.

---

## 3.7 Permissões

### Tabela: `RolePermissions`

Responsável pelo modelo RBAC (Role Based Access Control).

---

## 3.8 Associação de Utilizadores a Roles

### Tabela: `UserRoles`

Responsável pela atribuição de roles aos utilizadores.

---

## 3.9 Refresh Tokens

### Tabela: `RefreshTokens`

Responsável pela persistência de sessões autenticadas.

---

## 3.10 JWT Keys

### Tabela: `JwtKeys`

Responsável pela gestão criptográfica da plataforma.

---

## 3.11 Jobs do Sistema

### Tabela: `JobDefinitions`

Responsável pela configuração de jobs automatizados.

---

# 4. Modelo de Segurança

O sistema implementa:

- isolamento multi-tenant
- autenticação baseada em JWT
- refresh tokens
- RBAC
- soft delete
- rotação de chaves
- validação de integridade
- controlo de unicidade

---

# 5. Estratégia Multi-Tenant

Toda a estrutura da plataforma foi desenhada para garantir:

- isolamento lógico
- independência funcional
- segurança entre organizações

---

# 6. Estratégia de Auditoria

A maioria das tabelas possui:

- AddedBy
- AddedOn
- ModifiedBy
- ModifiedAt

---

# 7. Estratégia de Soft Delete

O sistema utiliza:

```text
IsDeleted
```

para remoção lógica de dados.

---

# 8. Estratégia de Performance

A base de dados possui índices dedicados para:

- autenticação
- lookup de permissões
- refresh tokens
- jobs ativos
- pesquisa de utilizadores
- associação de roles

---

# 9. Estratégia de Integridade

A integridade é garantida através de:

- primary keys
- foreign keys
- unique constraints
- check constraints
- validação JSON
- índices filtrados

---

# 10. Conclusão

A base de dados da aplicação **EBL.FIG.Process.Identity.API** foi desenhada para suportar:

- autenticação empresarial
- autorização granular
- multi-tenancy
- escalabilidade
- segurança
- auditoria
- extensibilidade
- automação operacional
