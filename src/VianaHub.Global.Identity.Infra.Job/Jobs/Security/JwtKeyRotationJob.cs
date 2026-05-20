using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Job.Interfaces;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Job.Jobs.Security;

/// <summary>
/// Job de rotação automática de chaves JWT
/// Executado diariamente às 03:00 UTC
/// </summary>
public class JwtKeyRotationJob : IJob
{
    private readonly IJwtKeyDomainService _jwtKeyService;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly ILogger<JwtKeyRotationJob> _logger;
    private readonly ILocalizationService _localization;
    private readonly IRequestTenantContext _tenantContext;

    /// <summary>
    /// ID do sistema para operações automatizadas
    /// </summary>
    private static readonly int SystemUserId = 0;

    public JwtKeyRotationJob(
        IJwtKeyDomainService jwtKeyService,
        ITenantDataRepository tenantRepo,
        ILogger<JwtKeyRotationJob> logger,
        ILocalizationService localization,
        IRequestTenantContext tenantContext)
    {
        _jwtKeyService = jwtKeyService;
        _tenantRepo = tenantRepo;
        _logger = logger;
        _localization = localization;
        _tenantContext = tenantContext;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[JwtKeyRotationJob] " + _localization.GetMessage("Job.SyncDefinitions.Starting"));

        try
        {
            // 0. Garantir que cada tenant ativo possua uma chave ativa (reconciliação mínima)
            var tenants = await _tenantRepo.GetAllAsync(cancellationToken);
            var activeTenants = tenants.Where(t => t.IsActive && !t.IsDeleted).ToList();

            _logger.LogInformation("[JwtKeyRotationJob] " + _localization.GetMessage("Job.SyncDefinitions.Starting") + " Verificando existência de chaves para {Count} tenants ativos", activeTenants.Count);

            var createdCount = 0;
            foreach (var tenant in activeTenants)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    _tenantContext.SetTenantId(tenant.Id);

                    var existing = await _jwtKeyService.GetActiveKeyAsync(tenant.Id, cancellationToken);
                    if (existing != null)
                    {
                        _logger.LogDebug("[JwtKeyRotationJob] Tenant {TenantId} já possui chave ativa (KeyId={KeyId})", tenant.Id, existing.KeyId);
                        continue;
                    }

                    var newKey = await _jwtKeyService.EnsureKeyExistsAsync(tenant.Id, SystemUserId, cancellationToken);
                    if (newKey != null)
                    {
                        createdCount++;
                        _logger.LogInformation("[JwtKeyRotationJob] Tenant {TenantId}: chave JWT criada (KeyId={KeyId})", tenant.Id, newKey.KeyId);
                    }
                    else
                    {
                        _logger.LogError("[JwtKeyRotationJob] Tenant {TenantId}: falha ao criar chave JWT", tenant.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[JwtKeyRotationJob] Erro ao garantir chave para Tenant {TenantId}", tenant.Id);
                }
                finally
                {
                    _tenantContext.Clear();
                }
            }

            if (createdCount > 0)
            {
                _logger.LogInformation("[JwtKeyRotationJob] Reconciliação de chaves finalizada. Chaves criadas: {CreatedCount}", createdCount);
            }

            // 1. Executar rotação para chaves elegíveis (por tenant, respeitando RLS)
            var rotatedCount = 0;
            foreach (var tenant in activeTenants)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    _tenantContext.SetTenantId(tenant.Id);
                    rotatedCount += await _jwtKeyService.RotateKeysAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[JwtKeyRotationJob] Erro ao rotacionar chaves para Tenant {TenantId}", tenant.Id);
                }
                finally
                {
                    _tenantContext.Clear();
                }
            }

            if (rotatedCount > 0)
            {
                _logger.LogInformation(_localization.GetMessage("Job.SyncDefinitions.Completed") + ": Rotação concluída com sucesso. Chaves rotacionadas: {RotatedCount}", rotatedCount);
            }
            else
            {
                _logger.LogInformation("[JwtKeyRotationJob] " + _localization.GetMessage("Job.SyncDefinitions.Completed") + " Nenhuma chave elegível para rotação encontrada");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JwtKeyRotationJob] " + _localization.GetMessage("Job.SyncDefinitions.Error"));
            throw;
        }
    }
}
