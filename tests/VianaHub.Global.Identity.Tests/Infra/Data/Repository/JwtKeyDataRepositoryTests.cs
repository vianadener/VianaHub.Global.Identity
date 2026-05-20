using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Infra.Data.Repository;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class JwtKeyDataRepositoryTests
{
    private const int TenantId = 1;

    private JwtKeyDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext(), NullLogger<JwtKeyDataRepository>.Instance);

    private JwtKeyDataRepository CreateSutWithData(params JwtKeyEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.JwtKeys.AddRange(entities);
        context.SaveChanges();
        return new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);
    }

    #region Construtor

    [Fact(DisplayName = "JwtKeyDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "JwtKeyDataRepository - Deve implementar IJwtKeyDataRepository")]
    [Trait("Infra.Data", "")]
    public void JwtKeyDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IJwtKeyDataRepository>(sut);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar JwtKey quando encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrada_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJwtKey(1, TenantId));

        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByKeyIdAsync

    [Fact(DisplayName = "GetByKeyIdAsync - Deve retornar JwtKey pelo KeyId")]
    [Trait("Infra.Data", "")]
    public async Task GetByKeyIdAsync_Encontrada_DeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByKeyIdAsync(entity.KeyId, default);

        Assert.NotNull(result);
        Assert.Equal(entity.KeyId, result.KeyId);
    }

    [Fact(DisplayName = "GetByKeyIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByKeyIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByKeyIdAsync(Guid.NewGuid(), default);

        Assert.Null(result);
    }

    #endregion

    #region GetActiveKeyAsync

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar chave ativa do tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetActiveKeyAsync_ChaveAtiva_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJwtKey(1, TenantId));

        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar null quando não há chave ativa")]
    [Trait("Infra.Data", "")]
    public async Task GetActiveKeyAsync_SemChaveAtiva_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        entity.Deactivate(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de JwtKeys")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId));

        var result = await sut.GetAllAsync(default);

        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há JwtKeys")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByTenantAsync

    [Fact(DisplayName = "GetByTenantAsync - Deve retornar JwtKeys do tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetByTenantAsync_Sucesso_DeveRetornarDoTenant()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, 99));

        var result = await sut.GetByTenantAsync(TenantId, default);

        Assert.Single(result);
    }

    #endregion

    #region GetByApplicationAsync

    [Fact(DisplayName = "GetByApplicationAsync - Deve retornar JwtKeys do tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetByApplicationAsync_Sucesso_DeveRetornarDoTenant()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, 99));

        var result = await sut.GetByApplicationAsync(TenantId, default);

        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetByApplicationAsync - Deve retornar lista vazia quando não há JwtKeys para o tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetByApplicationAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJwtKey(1, 99));

        var result = await sut.GetByApplicationAsync(TenantId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetByApplicationAsync - Não deve retornar JwtKeys deletadas")]
    [Trait("Infra.Data", "")]
    public async Task GetByApplicationAsync_ChaveDeletada_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByApplicationAsync(TenantId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetByApplicationAsync - Deve retornar lista em ordem decrescente por AddedOn")]
    [Trait("Infra.Data", "")]
    public async Task GetByApplicationAsync_Sucesso_DeveRetornarOrdenadoDecrescente()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, TenantId));

        var result = (await sut.GetByApplicationAsync(TenantId, default)).ToList();

        Assert.Equal(3, result.Count);
        Assert.True(result[0].AddedOn >= result[1].AddedOn);
    }

    #endregion

    #region GetKeysEligibleForRotationAsync

    [Fact(DisplayName = "GetKeysEligibleForRotationAsync - Deve retornar chaves elegíveis para rotação")]
    [Trait("Infra.Data", "")]
    public async Task GetKeysEligibleForRotationAsync_ChavesElegiveis_DeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.NextRotationAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-1));
        var sut = CreateSutWithData(entity);

        var result = await sut.GetKeysEligibleForRotationAsync(default);

        Assert.Single(result);
    }

    [Fact(DisplayName = "GetKeysEligibleForRotationAsync - Deve retornar vazio quando nenhuma chave elegível")]
    [Trait("Infra.Data", "")]
    public async Task GetKeysEligibleForRotationAsync_SemChavesElegiveis_DeveRetornarVazio()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.NextRotationAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(30));
        var sut = CreateSutWithData(entity);

        var result = await sut.GetKeysEligibleForRotationAsync(default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetKeysEligibleForRotationAsync - Não deve retornar chaves inativas")]
    [Trait("Infra.Data", "")]
    public async Task GetKeysEligibleForRotationAsync_ChaveInativa_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.NextRotationAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-1));
        entity.Deactivate(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetKeysEligibleForRotationAsync(default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetKeysEligibleForRotationAsync - Não deve retornar chaves deletadas")]
    [Trait("Infra.Data", "")]
    public async Task GetKeysEligibleForRotationAsync_ChaveDeletada_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.NextRotationAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-1));
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetKeysEligibleForRotationAsync(default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetKeysEligibleForRotationAsync - Não deve retornar chaves revogadas")]
    [Trait("Infra.Data", "")]
    public async Task GetKeysEligibleForRotationAsync_ChaveRevogada_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.NextRotationAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-1));
        entity.Revoke("Motivo", 1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetKeysEligibleForRotationAsync(default);

        Assert.Empty(result);
    }

    #endregion

    #region GetExpiredKeysAsync

    [Fact(DisplayName = "GetExpiredKeysAsync - Deve retornar chaves expiradas além do período de retenção")]
    [Trait("Infra.Data", "")]
    public async Task GetExpiredKeysAsync_ChavesExpiradas_DeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        entity.Deactivate(1);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-60));
        var sut = CreateSutWithData(entity);

        var result = await sut.GetExpiredKeysAsync(retentionDays: 30, default);

        Assert.Single(result);
    }

    [Fact(DisplayName = "GetExpiredKeysAsync - Não deve retornar chaves dentro do período de retenção")]
    [Trait("Infra.Data", "")]
    public async Task GetExpiredKeysAsync_DentroRetencao_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        entity.Deactivate(1);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-10));
        var sut = CreateSutWithData(entity);

        var result = await sut.GetExpiredKeysAsync(retentionDays: 30, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetExpiredKeysAsync - Não deve retornar chaves deletadas")]
    [Trait("Infra.Data", "")]
    public async Task GetExpiredKeysAsync_ChaveDeletada_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-60));
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetExpiredKeysAsync(retentionDays: 30, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetExpiredKeysAsync - Não deve retornar chaves ainda ativas")]
    [Trait("Infra.Data", "")]
    public async Task GetExpiredKeysAsync_ChaveAtiva_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        typeof(JwtKeyEntity).GetProperty(nameof(JwtKeyEntity.ExpiresAt))!
            .SetValue(entity, DateTime.UtcNow.AddDays(-60));
        var sut = CreateSutWithData(entity);

        var result = await sut.GetExpiredKeysAsync(retentionDays: 30, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetExpiredKeysAsync - Deve retornar vazio quando não há chaves expiradas")]
    [Trait("Infra.Data", "")]
    public async Task GetExpiredKeysAsync_SemChavesExpiradas_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetExpiredKeysAsync(retentionDays: 30, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página com itens")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginaComItens()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, TenantId));
        var filter = new PagedFilter(string.Empty, null, 1, 10, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página vazia quando não há dados")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_SemDados_DeveRetornarPaginaVazia()
    {
        var sut = CreateSut();
        var filter = new PagedFilter(string.Empty, null, 1, 10, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve respeitar o tamanho da página")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRespeitarPageSize()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, TenantId),
            RepositoryTestHelper.BuildJwtKey(4, TenantId),
            RepositoryTestHelper.BuildJwtKey(5, TenantId));
        var filter = new PagedFilter(string.Empty, null, 1, 2, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(5, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve respeitar o número da página")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRespeitarPageNumber()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, TenantId),
            RepositoryTestHelper.BuildJwtKey(4, TenantId),
            RepositoryTestHelper.BuildJwtKey(5, TenantId));
        var filter = new PagedFilter(string.Empty, null, 2, 2, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.PageNumber);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve calcular TotalPages corretamente")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveCalcularTotalPagesCorretamente()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId),
            RepositoryTestHelper.BuildJwtKey(3, TenantId));
        var filter = new PagedFilter(string.Empty, null, 1, 2, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.Equal(2, result.TotalPages);
    }

    [Fact(DisplayName = "GetPagedAsync - Não deve retornar itens deletados")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComDeletedItems_NaoDeveRetornar()
    {
        var deleted = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        deleted.Delete(1);
        var sut = CreateSutWithData(
            deleted,
            RepositoryTestHelper.BuildJwtKey(2, TenantId));
        var filter = new PagedFilter(string.Empty, null, 1, 10, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.Equal(1, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve lançar InvalidOperationException ao filtrar por Search com propriedade não mapeada")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComSearch_DeveLancarInvalidOperationException()
    {
        var entity1 = new JwtKeyEntity(TenantId, "RSA-PUBLIC-KEY-ESPECIAL", "privKey", 1);
        RepositoryTestHelper.SetId(entity1, 1);
        var sut = CreateSutWithData(entity1);
        var filter = new PagedFilter("ESPECIAL", null, 1, 10, string.Empty, string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetPagedAsync(filter, TenantId, default));
    }

    [Fact(DisplayName = "GetPagedAsync - Deve retornar todos os itens quando Search está vazio")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_SearchVazio_DeveRetornarTodos()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJwtKey(1, TenantId),
            RepositoryTestHelper.BuildJwtKey(2, TenantId));
        var filter = new PagedFilter(string.Empty, null, 1, 10, string.Empty, string.Empty);

        var result = await sut.GetPagedAsync(filter, TenantId, default);

        Assert.Equal(2, result.TotalItems);
    }

    #endregion

    #region BulkUpdateTelemetryAsync

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve atualizar telemetria de múltiplos registros")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_Sucesso_DeveAtualizarTelemetria()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity1 = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        var entity2 = RepositoryTestHelper.BuildJwtKey(2, TenantId);
        context.JwtKeys.AddRange(entity1, entity2);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        var now = DateTime.UtcNow;
        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>
        {
            (1, 10, now, 5, now),
            (2, 20, now, 8, now)
        };

        var result = await sut.BulkUpdateTelemetryAsync(updates, default);

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve retornar zero quando nenhum Id é encontrado")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_IdInexistente_DeveRetornarZero()
    {
        var sut = CreateSut();
        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>
        {
            (999, 5, null, 1, null)
        };

        var result = await sut.BulkUpdateTelemetryAsync(updates, default);

        Assert.Equal(0, result);
    }

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve retornar zero quando lista de updates está vazia")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_ListaVazia_DeveRetornarZero()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJwtKey(1, TenantId));
        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>();

        var result = await sut.BulkUpdateTelemetryAsync(updates, default);

        Assert.Equal(0, result);
    }

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve atualizar UsageCount corretamente")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_Sucesso_DeveAtualizarUsageCount()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        context.JwtKeys.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        var now = DateTime.UtcNow;
        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>
        {
            (1, 42, now, 7, now)
        };

        await sut.BulkUpdateTelemetryAsync(updates, default);

        var updated = await context.JwtKeys.FindAsync(1);
        Assert.Equal(42, updated!.UsageCount);
    }

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve atualizar ValidationCount corretamente")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_Sucesso_DeveAtualizarValidationCount()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        context.JwtKeys.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        var now = DateTime.UtcNow;
        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>
        {
            (1, 0, null, 99, now)
        };

        await sut.BulkUpdateTelemetryAsync(updates, default);

        var updated = await context.JwtKeys.FindAsync(1);
        Assert.Equal(99, updated!.ValidationCount);
    }

    [Fact(DisplayName = "BulkUpdateTelemetryAsync - Deve aceitar LastUsedAt e LastValidatedAt nulos")]
    [Trait("Infra.Data", "")]
    public async Task BulkUpdateTelemetryAsync_DatasNulas_NaoDeveLancarExcecao()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        context.JwtKeys.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        var updates = new List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)>
        {
            (1, 5, null, 3, null)
        };

        var exception = await Record.ExceptionAsync(() => sut.BulkUpdateTelemetryAsync(updates, default));

        Assert.Null(exception);
    }

    #endregion

    #region HasActiveKeyAsync

    [Fact(DisplayName = "HasActiveKeyAsync - Deve retornar true quando há chave ativa")]
    [Trait("Infra.Data", "")]
    public async Task HasActiveKeyAsync_ChaveAtiva_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJwtKey(1, TenantId));

        var result = await sut.HasActiveKeyAsync(TenantId, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "HasActiveKeyAsync - Deve retornar false quando não há chave ativa")]
    [Trait("Infra.Data", "")]
    public async Task HasActiveKeyAsync_SemChaveAtiva_DeveRetornarFalse()
    {
        var entity = RepositoryTestHelper.BuildJwtKey(1, TenantId);
        entity.Deactivate(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.HasActiveKeyAsync(TenantId, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir JwtKey com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new JwtKeyEntity(TenantId, "pubKey", "privKey", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar JwtKey com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new JwtKeyEntity(TenantId, "pubKey", "privKey", 1);
        context.JwtKeys.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        entity.Deactivate(1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar JwtKey com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new JwtKeyEntity(TenantId, "pubKey", "privKey", 1);
        context.JwtKeys.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JwtKeyDataRepository(context, NullLogger<JwtKeyDataRepository>.Instance);

        entity.Delete(1);
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
