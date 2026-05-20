using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Pagination;

public class CreateSortTests
{
    private class SampleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public SampleNested Nested { get; set; } = new();
    }

    private class SampleNested
    {
        public string Value { get; set; } = string.Empty;
    }

    #region SortBy

    [Fact(DisplayName = "SortBy - Deve retornar expressão válida para propriedade existente")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeExistente_DeveRetornarExpressaoValida()
    {
        var expression = CreateSort.SortBy<SampleEntity>("Name");

        Assert.NotNull(expression);
    }

    [Fact(DisplayName = "SortBy - Deve ordenar corretamente por propriedade existente")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeExistente_DeveOrdenarCorretamente()
    {
        var items = new List<SampleEntity>
        {
            new() { Id = 1, Name = "Charlie" },
            new() { Id = 2, Name = "Alice" },
            new() { Id = 3, Name = "Bob" }
        }.AsQueryable();

        var expression = CreateSort.SortBy<SampleEntity>("Name");
        var ordered = items.OrderBy(expression).ToList();

        Assert.Equal("Alice", ordered[0].Name);
        Assert.Equal("Bob", ordered[1].Name);
        Assert.Equal("Charlie", ordered[2].Name);
    }

    [Fact(DisplayName = "SortBy - Deve ser case-insensitive no nome da propriedade")]
    [Trait("Domain", "")]
    public void SortBy_NomePropriedadeMaiusculo_DeveRetornarExpressaoValida()
    {
        var expression = CreateSort.SortBy<SampleEntity>("NAME");

        Assert.NotNull(expression);
    }

    [Fact(DisplayName = "SortBy - Deve retornar fallback para propriedade inexistente")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeInexistente_DeveRetornarFallback()
    {
        var items = new List<SampleEntity>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        }.AsQueryable();

        var expression = CreateSort.SortBy<SampleEntity>("PropriedadeQueNaoExiste");

        Assert.NotNull(expression);
        var result = items.OrderBy(expression).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "SortBy - Deve retornar expressão constante para entrada nula ou vazia")]
    [Trait("Domain", "")]
    public void SortBy_EntradaVazia_DeveRetornarExpressaoConstante()
    {
        var items = new List<SampleEntity>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        }.AsQueryable();

        var expression = CreateSort.SortBy<SampleEntity>(string.Empty);

        Assert.NotNull(expression);
        var result = items.OrderBy(expression).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "SortBy - Deve retornar expressão constante para entrada nula")]
    [Trait("Domain", "")]
    public void SortBy_EntradaNula_DeveRetornarExpressaoConstante()
    {
        var expression = CreateSort.SortBy<SampleEntity>(null!);

        Assert.NotNull(expression);
    }

    [Fact(DisplayName = "SortBy - Deve retornar expressão válida para propriedade aninhada")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeAninhada_DeveRetornarExpressaoValida()
    {
        var expression = CreateSort.SortBy<SampleEntity>("Nested.Value");

        Assert.NotNull(expression);
    }

    [Fact(DisplayName = "SortBy - Deve retornar fallback para propriedade aninhada inexistente")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeAninhadaInexistente_DeveRetornarFallback()
    {
        var expression = CreateSort.SortBy<SampleEntity>("Nested.Inexistente");

        Assert.NotNull(expression);
    }

    [Fact(DisplayName = "SortBy - Deve usar fallback para Id quando propriedade não existe e Id está disponível")]
    [Trait("Domain", "")]
    public void SortBy_PropriedadeInexistente_DeveUsarFallbackId()
    {
        var items = new List<SampleEntity>
        {
            new() { Id = 3 },
            new() { Id = 1 },
            new() { Id = 2 }
        }.AsQueryable();

        var expression = CreateSort.SortBy<SampleEntity>("CampoInexistente");
        var result = items.OrderBy(expression).ToList();

        Assert.Equal(3, result.Count);
    }

    #endregion
}
