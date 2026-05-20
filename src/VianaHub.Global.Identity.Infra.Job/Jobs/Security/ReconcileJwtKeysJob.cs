using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Job.Interfaces;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Job.Jobs.Security;

/// <summary>
/// Job de reconciliação de chaves JWT para aplicações legadas.
/// Garante que todas as combinações Tenant + Application ativas tenham pelo menos uma chave JWT ativa.
/// Executa uma única vez na inicialização ou sob demanda.
/// </summary>
public class ReconcileJwtKeysJob : IJob
{
    private readonly IJwtKeyDomainService _jwtKeyDomain;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly IJwtKeyDataRepository _jwtKeyRepo;
    private readonly IRequestTenantContext _tenantContext;
    private readonly ILogger<ReconcileJwtKeysJob> _logger;

    /// <summary>
    /// ID do sistema para operações automatizadas
    /// </summary>
    private static readonly int SystemUserId = 0;

    public ReconcileJwtKeysJob(
        IJwtKeyDomainService jwtKeyDomain,
        ITenantDataRepository tenantRepo,
        IJwtKeyDataRepository jwtKeyRepo,
        IRequestTenantContext tenantContext,
        ILogger<ReconcileJwtKeysJob> logger)
    {
        _jwtKeyDomain = jwtKeyDomain;
        _tenantRepo = tenantRepo;
        _jwtKeyRepo = jwtKeyRepo;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? [ReconcileJwtKeysJob] Iniciando reconciliação de chaves JWT para aplicações legadas");

        var startTime = DateTime.UtcNow;
        var totalProcessed = 0;
        var totalCreated = 0;
        var totalSkipped = 0;
        var totalErrors = 0;

        // 1. Buscar todos os tenants ativos
        var tenants = await _tenantRepo.GetAllAsync(cancellationToken);
        var activeTenants = tenants.Where(t => t.IsActive && !t.IsDeleted).ToList();

        _logger.LogInformation("?? [ReconcileJwtKeysJob] Encontrados {TenantCount} tenants ativos para processar", activeTenants.Count);

        // 2. Processar tenants ativos
        foreach (var tenant in activeTenants)
        {
            try
            {
                _tenantContext.SetTenantId(tenant.Id);
                var result = await ProcessTenantAsync(tenant.Id, tenant.Name, cancellationToken);

                totalProcessed += result.Processed;
                totalCreated += result.Created;
                totalSkipped += result.Skipped;
                totalErrors += result.Errors;
            }
            finally
            {
                _tenantContext.Clear();
            }
        }

        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation(
            "? [ReconcileJwtKeysJob] Reconciliação concluída com sucesso. " +
            "Processados: {Processed}, Criados: {Created}, Pulados: {Skipped}, Erros: {Errors}, Duração: {Duration}s",
            totalProcessed, totalCreated, totalSkipped, totalErrors, duration.TotalSeconds);
    }

    /// <summary>
    /// Processa o tenant específico, verificando e criando chave JWT se necessário
    /// </summary>
    private async Task<ProcessingTenantResult> ProcessTenantAsync(int tenantId, string tenantName, CancellationToken cancellationToken)
    {
        var result = new ProcessingTenantResult();
        result.Processed++;

        var existingKey = await _jwtKeyRepo.GetActiveKeyAsync(tenantId, cancellationToken);

        if (existingKey != null)
        {
            _logger.LogDebug(
                "[ReconcileJwtKeysJob] Tenant {TenantId} ({TenantName}): chave JWT ativa já existe (KeyId: {KeyId}), pulando",
                tenantId, tenantName, existingKey.KeyId);
            result.Skipped++;
            return result;
        }

        _logger.LogInformation(
            "[ReconcileJwtKeysJob] Tenant {TenantId} ({TenantName}): nenhuma chave JWT encontrada, criando automaticamente",
            tenantId, tenantName);

        var newKey = await _jwtKeyDomain.EnsureKeyExistsAsync(tenantId, SystemUserId, cancellationToken);

        if (newKey != null)
        {
            _logger.LogInformation(
                "[ReconcileJwtKeysJob] Tenant {TenantId} ({TenantName}): chave JWT criada com sucesso (KeyId: {KeyId})",
                tenantId, tenantName, newKey.KeyId);
            result.Created++;
        }
        else
        {
            _logger.LogWarning(
                "[ReconcileJwtKeysJob] Tenant {TenantId} ({TenantName}): falha ao criar chave JWT (resultado nulo)",
                tenantId, tenantName);
            result.Errors++;
        }

        return result;
    }

    /// <summary>
    /// Resultado do processamento de aplicações
    /// </summary>
    private class ProcessingTenantResult
    {
        public int Processed { get; set; }
        public int Created { get; set; }
        public int Skipped { get; set; }
        public int Errors { get; set; }
    }
}
