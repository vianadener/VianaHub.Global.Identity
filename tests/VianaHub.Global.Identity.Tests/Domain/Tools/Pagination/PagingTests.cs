using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Pagination;

public class PagingTests
{
    #region PageNumber

    [Fact(DisplayName = "PageNumber - Deve retornar 1 quando não definido")]
    [Trait("Domain", "")]
    public void PageNumber_NaoDefinido_DeveRetornar1()
    {
        var paging = new Paging();

        Assert.Equal(1, paging.PageNumber);
    }

    [Fact(DisplayName = "PageNumber - Deve retornar 1 quando definido como zero")]
    [Trait("Domain", "")]
    public void PageNumber_Zero_DeveRetornar1()
    {
        var paging = new Paging { PageNumber = 0 };

        Assert.Equal(1, paging.PageNumber);
    }

    [Fact(DisplayName = "PageNumber - Deve retornar 1 quando definido como negativo")]
    [Trait("Domain", "")]
    public void PageNumber_Negativo_DeveRetornar1()
    {
        var paging = new Paging { PageNumber = -5 };

        Assert.Equal(1, paging.PageNumber);
    }

    [Fact(DisplayName = "PageNumber - Deve retornar valor definido quando positivo")]
    [Trait("Domain", "")]
    public void PageNumber_Positivo_DeveRetornarValorDefinido()
    {
        var paging = new Paging { PageNumber = 3 };

        Assert.Equal(3, paging.PageNumber);
    }

    #endregion

    #region PageSize

    [Fact(DisplayName = "PageSize - Deve retornar MinPageSize quando não definido")]
    [Trait("Domain", "")]
    public void PageSize_NaoDefinido_DeveRetornarMinPageSize()
    {
        var paging = new Paging();

        Assert.Equal(Paging.MinPageSize(), paging.PageSize);
    }

    [Fact(DisplayName = "PageSize - Deve retornar MinPageSize quando definido como zero")]
    [Trait("Domain", "")]
    public void PageSize_Zero_DeveRetornarMinPageSize()
    {
        var paging = new Paging { PageSize = 0 };

        Assert.Equal(Paging.MinPageSize(), paging.PageSize);
    }

    [Fact(DisplayName = "PageSize - Deve retornar MinPageSize quando definido como negativo")]
    [Trait("Domain", "")]
    public void PageSize_Negativo_DeveRetornarMinPageSize()
    {
        var paging = new Paging { PageSize = -10 };

        Assert.Equal(Paging.MinPageSize(), paging.PageSize);
    }

    [Fact(DisplayName = "PageSize - Deve retornar valor definido quando dentro dos limites")]
    [Trait("Domain", "")]
    public void PageSize_DentroDoLimite_DeveRetornarValorDefinido()
    {
        var paging = new Paging { PageSize = 50 };

        Assert.Equal(50, paging.PageSize);
    }

    [Fact(DisplayName = "PageSize - Deve retornar MaxPageSize quando definido acima do máximo")]
    [Trait("Domain", "")]
    public void PageSize_AcimaDoMaximo_DeveRetornarMaxPageSize()
    {
        var paging = new Paging { PageSize = 9999 };

        Assert.Equal(Paging.MaxPageSize(), paging.PageSize);
    }

    [Fact(DisplayName = "PageSize - Deve retornar exatamente MaxPageSize quando definido igual ao máximo")]
    [Trait("Domain", "")]
    public void PageSize_IgualAoMaximo_DeveRetornarMaxPageSize()
    {
        var paging = new Paging { PageSize = Paging.MaxPageSize() };

        Assert.Equal(Paging.MaxPageSize(), paging.PageSize);
    }

    #endregion

    #region MaxPageSize / MinPageSize

    [Fact(DisplayName = "MaxPageSize - Deve retornar 1000")]
    [Trait("Domain", "")]
    public void MaxPageSize_DeveRetornar1000()
    {
        Assert.Equal(1000, Paging.MaxPageSize());
    }

    [Fact(DisplayName = "MinPageSize - Deve retornar 10")]
    [Trait("Domain", "")]
    public void MinPageSize_DeveRetornar10()
    {
        Assert.Equal(10, Paging.MinPageSize());
    }

    #endregion

    #region Herança Order

    [Fact(DisplayName = "Paging - Deve herdar comportamento padrão de SortBy do Order")]
    [Trait("Domain", "")]
    public void Paging_SortByNaoDefinido_DeveRetornarAddedOn()
    {
        var paging = new Paging();

        Assert.Equal("AddedOn", paging.SortBy);
    }

    [Fact(DisplayName = "Paging - Deve herdar comportamento padrão de SortDirection do Order")]
    [Trait("Domain", "")]
    public void Paging_SortDirectionNaoDefinido_DeveRetornarDesc()
    {
        var paging = new Paging();

        Assert.Equal("desc", paging.SortDirection);
    }

    #endregion
}
