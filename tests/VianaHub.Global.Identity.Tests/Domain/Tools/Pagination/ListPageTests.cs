using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Pagination;

public class ListPageTests
{
    #region Constructor

    [Fact(DisplayName = "ListPage - Construtor padrão deve criar instância com valores default")]
    [Trait("Domain", "")]
    public void ListPage_ConstrutorPadrao_DeveCriarInstanciaComValoresDefault()
    {
        var listPage = new ListPage<string>();

        Assert.Null(listPage.Items);
        Assert.Equal(0, listPage.PageNumber);
        Assert.Equal(0, listPage.PageSize);
        Assert.Equal(0, listPage.TotalItems);
        Assert.Equal(0, listPage.TotalPages);
    }

    [Fact(DisplayName = "ListPage - Construtor parametrizado deve atribuir valores corretamente")]
    [Trait("Domain", "")]
    public void ListPage_ConstrutorParametrizado_DeveAtribuirValoresCorretamente()
    {
        var items = new List<string> { "a", "b", "c" };

        var listPage = new ListPage<string>(items, 1, 10, 3, 1);

        Assert.Equal(items, listPage.Items);
        Assert.Equal(1, listPage.PageNumber);
        Assert.Equal(10, listPage.PageSize);
        Assert.Equal(3, listPage.TotalItems);
        Assert.Equal(1, listPage.TotalPages);
    }

    [Fact(DisplayName = "ListPage - Deve suportar lista vazia de itens")]
    [Trait("Domain", "")]
    public void ListPage_ListaVazia_DeveCriarInstanciaValida()
    {
        var listPage = new ListPage<int>([], 1, 10, 0, 0);

        Assert.Empty(listPage.Items);
        Assert.Equal(0, listPage.TotalItems);
        Assert.Equal(0, listPage.TotalPages);
    }

    [Fact(DisplayName = "ListPage - Deve suportar tipos de referência genéricos")]
    [Trait("Domain", "")]
    public void ListPage_TipoReferencia_DeveFuncionarCorretamente()
    {
        var items = new List<object> { new(), new() };

        var listPage = new ListPage<object>(items, 2, 5, 2, 1);

        Assert.Equal(2, listPage.Items.Count());
        Assert.Equal(2, listPage.PageNumber);
    }

    #endregion

    #region Properties

    [Fact(DisplayName = "ListPage - Deve permitir alterar propriedades após criação")]
    [Trait("Domain", "")]
    public void ListPage_AlterarPropriedades_DeveRefletirNovosValores()
    {
        var listPage = new ListPage<string>
        {
            Items = ["x"],
            PageNumber = 3,
            PageSize = 20,
            TotalItems = 100,
            TotalPages = 5
        };

        Assert.Single(listPage.Items);
        Assert.Equal(3, listPage.PageNumber);
        Assert.Equal(20, listPage.PageSize);
        Assert.Equal(100, listPage.TotalItems);
        Assert.Equal(5, listPage.TotalPages);
    }

    #endregion
}
