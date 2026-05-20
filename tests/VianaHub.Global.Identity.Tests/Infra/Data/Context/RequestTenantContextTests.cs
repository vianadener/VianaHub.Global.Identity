namespace VianaHub.Global.Identity.Tests.Infra.Data.Context;

public class RequestTenantContextTests
{
    #region TenantId

    [Fact(DisplayName = "TenantId - Deve retornar null quando nenhum tenant foi definido")]
    [Trait("Infra.Data", "")]
    public void TenantId_SemDefinicao_DeveRetornarNull()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();

        Assert.Null(sut.TenantId);
    }

    #endregion

    #region SetTenantId

    [Fact(DisplayName = "SetTenantId - Deve definir o TenantId corretamente")]
    [Trait("Infra.Data", "")]
    public void SetTenantId_Sucesso_DeveDefinirTenantId()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();

        sut.SetTenantId(42);

        Assert.Equal(42, sut.TenantId);
    }

    [Fact(DisplayName = "SetTenantId - Deve sobrescrever TenantId quando chamado novamente")]
    [Trait("Infra.Data", "")]
    public void SetTenantId_ChamadoNovamente_DeveSubstituirTenantId()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();
        sut.SetTenantId(10);

        sut.SetTenantId(99);

        Assert.Equal(99, sut.TenantId);
    }

    #endregion

    #region Clear

    [Fact(DisplayName = "Clear - Deve limpar o TenantId definido")]
    [Trait("Infra.Data", "")]
    public void Clear_ComTenantDefinido_DeveRetornarNull()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();
        sut.SetTenantId(5);

        sut.Clear();

        Assert.Null(sut.TenantId);
    }

    [Fact(DisplayName = "Clear - Não deve lançar exceção quando TenantId não foi definido")]
    [Trait("Infra.Data", "")]
    public void Clear_SemTenantDefinido_NaoDeveLancarExcecao()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();

        var exception = Record.Exception(() => sut.Clear());

        Assert.Null(exception);
        Assert.Null(sut.TenantId);
    }

    #endregion

    #region IRequestTenantContext

    [Fact(DisplayName = "RequestTenantContext - Deve implementar IRequestTenantContext")]
    [Trait("Infra.Data", "")]
    public void RequestTenantContext_DeveImplementarInterface()
    {
        var sut = new VianaHub.Global.Identity.Infra.Data.Context.RequestTenantContext();

        Assert.IsAssignableFrom<VianaHub.Global.Identity.Domain.Interfaces.Base.IRequestTenantContext>(sut);
    }

    #endregion
}
