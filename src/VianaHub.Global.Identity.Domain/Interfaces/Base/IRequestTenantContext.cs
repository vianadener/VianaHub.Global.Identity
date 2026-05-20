namespace VianaHub.Global.Identity.Domain.Interfaces.Base;

public interface IRequestTenantContext
{
    /// <summary>
    /// TenantId definido explicitamente para o request atual.
    /// Retorna null se nenhum tenant foi definido via SetTenantId.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Define o TenantId para o request atual.
    /// Deve ser chamado antes de qualquer acesso ao banco de dados.
    /// </summary>
    void SetTenantId(int tenantId);

    /// <summary>
    /// Remove o TenantId definido para o request atual.
    /// </summary>
    void Clear();
}
