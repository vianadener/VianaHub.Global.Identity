# Configuração de Segurança - JWT Master Key

## Visão Geral

A aplicação utiliza uma **JWT Master Key** para criptografar e descriptografar chaves privadas JWT armazenadas no banco de dados. Esta chave é essencial para o funcionamento correto do sistema de autenticação.

## Configuração em Desenvolvimento

### Opção 1: Variável de Ambiente (Recomendado)

```powershell
# Windows PowerShell
$env:JWT_MASTER_KEY = "dev-master-key-32-chars-min-length-required-for-security"

# Windows CMD
set JWT_MASTER_KEY=dev-master-key-32-chars-min-length-required-for-security
```

### Opção 2: appsettings.Development.json

```json
{
  "Security": {
    "JwtMasterKey": "dev-master-key-32-chars-min-length-required-for-security"
  }
}
```

⚠️ **IMPORTANTE**: Nunca commitar chaves reais no controle de versão!

## Configuração em Produção

### Azure Key Vault (Recomendado)

1. Crie um secret no Azure Key Vault com o nome `JwtMasterKey`
2. Configure o acesso ao Key Vault no `Program.cs`:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Variável de Ambiente (Alternativa)

Configure a variável de ambiente no servidor/container:

```bash
export JWT_MASTER_KEY="your-production-key-use-strong-random-value-min-32-chars"
```

## Geração de Chave Segura

Para gerar uma chave mestra segura, use:

```powershell
# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

# Ou usando openssl
openssl rand -base64 32
```

## Ordem de Precedência

O sistema busca a chave na seguinte ordem:

1. Variável de ambiente `JWT_MASTER_KEY`
2. Configuração `Security:JwtMasterKey` no appsettings
3. Configuração `JwtMasterKey` no appsettings (fallback)

Se nenhuma for encontrada, uma exceção será lançada durante a inicialização da aplicação.

## Implementação

A implementação está localizada em:
- Interface: `EBL.FIG.Process.Identity.Domain.Interfaces.Base.ISecretProvider`
- Implementação: `EBL.FIG.Process.Identity.Infra.Data.Providers.EnvironmentSecretProvider`
- Registro DI: `EBL.FIG.Process.Identity.Infra.IoC.DependencyInjection`

## Melhores Práticas

✅ **Faça:**
- Use Azure Key Vault ou AWS Secrets Manager em produção
- Use chaves com no mínimo 32 caracteres
- Rotacione as chaves periodicamente
- Use variáveis de ambiente em desenvolvimento

❌ **Não faça:**
- Committar chaves no código-fonte
- Usar chaves fracas ou previsíveis
- Compartilhar a mesma chave entre ambientes
- Hardcodar chaves no código
