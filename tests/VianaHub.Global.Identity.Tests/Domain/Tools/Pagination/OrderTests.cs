using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Pagination;

public class OrderTests
{
    #region SortBy

    [Fact(DisplayName = "SortBy - Deve retornar 'AddedOn' quando não definido")]
    [Trait("Domain", "")]
    public void SortBy_NaoDefinido_DeveRetornarAddedOn()
    {
        var order = new Order();

        Assert.Equal("AddedOn", order.SortBy);
    }

    [Fact(DisplayName = "SortBy - Deve retornar valor definido quando não vazio")]
    [Trait("Domain", "")]
    public void SortBy_ValorDefinido_DeveRetornarValor()
    {
        var order = new Order { SortBy = "Name" };

        Assert.Equal("Name", order.SortBy);
    }

    [Fact(DisplayName = "SortBy - Deve retornar 'AddedOn' quando definido como string vazia")]
    [Trait("Domain", "")]
    public void SortBy_StringVazia_DeveRetornarAddedOn()
    {
        var order = new Order { SortBy = string.Empty };

        Assert.Equal("AddedOn", order.SortBy);
    }

    [Fact(DisplayName = "SortBy - Deve retornar 'AddedOn' quando definido como espaço em branco")]
    [Trait("Domain", "")]
    public void SortBy_EspacoBranco_DeveRetornarAddedOn()
    {
        var order = new Order { SortBy = "   " };

        Assert.Equal("AddedOn", order.SortBy);
    }

    #endregion

    #region SortDirection

    [Fact(DisplayName = "SortDirection - Deve retornar 'desc' quando não definido")]
    [Trait("Domain", "")]
    public void SortDirection_NaoDefinido_DeveRetornarDesc()
    {
        var order = new Order();

        Assert.Equal("desc", order.SortDirection);
    }

    [Fact(DisplayName = "SortDirection - Deve retornar 'asc' quando definido como 'asc'")]
    [Trait("Domain", "")]
    public void SortDirection_Asc_DeveRetornarAsc()
    {
        var order = new Order { SortDirection = "asc" };

        Assert.Equal("asc", order.SortDirection);
    }

    [Fact(DisplayName = "SortDirection - Deve retornar 'desc' quando definido como 'desc'")]
    [Trait("Domain", "")]
    public void SortDirection_Desc_DeveRetornarDesc()
    {
        var order = new Order { SortDirection = "desc" };

        Assert.Equal("desc", order.SortDirection);
    }

    [Fact(DisplayName = "SortDirection - Deve retornar 'asc' para 'ASC' maiúsculo")]
    [Trait("Domain", "")]
    public void SortDirection_AscMaiusculo_DeveRetornarAsc()
    {
        var order = new Order { SortDirection = "ASC" };

        Assert.Equal("ASC", order.SortDirection);
    }

    [Fact(DisplayName = "SortDirection - Deve retornar 'desc' para valor inválido")]
    [Trait("Domain", "")]
    public void SortDirection_ValorInvalido_DeveRetornarDesc()
    {
        var order = new Order { SortDirection = "crescente" };

        Assert.Equal("desc", order.SortDirection);
    }

    [Fact(DisplayName = "SortDirection - Deve retornar 'desc' para string vazia")]
    [Trait("Domain", "")]
    public void SortDirection_StringVazia_DeveRetornarDesc()
    {
        var order = new Order { SortDirection = string.Empty };

        Assert.Equal("desc", order.SortDirection);
    }

    #endregion
}
