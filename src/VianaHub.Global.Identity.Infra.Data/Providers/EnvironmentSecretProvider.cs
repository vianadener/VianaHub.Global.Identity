using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.Extensions.Configuration;

namespace VianaHub.Global.Identity.Infra.Data.Providers;

/// <summary>
/// Implementação do ISecretProvider que busca secrets de variáveis de ambiente ou configuração.
/// Em produção, recomenda-se usar Azure Key Vault ou AWS Secrets Manager.
/// </summary>
public class EnvironmentSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;

    public EnvironmentSecretProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Recupera a chave mestra utilizada para criptografar/descriptografar chaves privadas JWT.
    /// Busca primeiro em variáveis de ambiente, depois em configuração.
    /// </summary>
    /// <returns>Chave mestra em texto simples ou null se não disponível.</returns>
    public string GetMasterKey()
    {
        // Tenta buscar da variável de ambiente primeiro
        var masterKey = Environment.GetEnvironmentVariable("JWT_MASTER_KEY");

        // Se não encontrar, tenta buscar da configuração
        if (string.IsNullOrEmpty(masterKey))
        {
            masterKey = _configuration["Security:JwtMasterKey"];
        }

        // Se ainda não encontrar, tenta buscar de Secrets do User Secrets ou Azure Key Vault
        if (string.IsNullOrEmpty(masterKey))
        {
            masterKey = _configuration["JwtMasterKey"];
        }

        return masterKey ?? throw new InvalidOperationException(
            "JWT Master Key não configurada. Configure a variável de ambiente 'JWT_MASTER_KEY' " +
            "ou a chave 'Security:JwtMasterKey' no appsettings.json. " +
            "Em produção, utilize Azure Key Vault ou AWS Secrets Manager.");
    }
}
