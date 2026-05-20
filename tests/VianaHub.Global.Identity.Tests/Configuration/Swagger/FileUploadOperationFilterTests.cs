using VianaHub.Global.Identity.Api.Configuration.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace VianaHub.Global.Identity.Tests.Configuration.Swagger;

public class FileUploadOperationFilterTests
{
    private static OperationFilterContext CreateContext(IEnumerable<ApiParameterDescription> parameters)
    {
        var apiDescription = new ApiDescription();
        foreach (var p in parameters)
            apiDescription.ParameterDescriptions.Add(p);

        var schemaGenerator = new Mock<ISchemaGenerator>().Object;
        var schemaRepository = new SchemaRepository();
        var methodInfo = typeof(FileUploadOperationFilterTests)
            .GetMethod(nameof(CreateContext), BindingFlags.NonPublic | BindingFlags.Static)!;

        return new OperationFilterContext(apiDescription, schemaGenerator, schemaRepository, methodInfo);
    }

    private static ApiParameterDescription CreateFileParameter(string name, Type type, bool isRequired = false)
    {
        var mockMetadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(type));

        return new ApiParameterDescription
        {
            Name = name,
            IsRequired = isRequired,
            ModelMetadata = mockMetadata.Object
        };
    }

    private static ApiParameterDescription CreateNonFileParameter(string name)
    {
        var mockMetadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(string)));

        return new ApiParameterDescription
        {
            Name = name,
            IsRequired = false,
            ModelMetadata = mockMetadata.Object
        };
    }

    #region Sucesso

    [Fact(DisplayName = "Apply - Não deve alterar operação quando não há parâmetros de arquivo")]
    [Trait("Api", "")]
    public void Apply_SemParametrosDeArquivo_NaoAlteraOperacao()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation();
        var context = CreateContext([CreateNonFileParameter("name")]);

        filter.Apply(operation, context);

        Assert.Null(operation.RequestBody);
    }

    [Fact(DisplayName = "Apply - Deve criar RequestBody multipart/form-data para IFormFile")]
    [Trait("Api", "")]
    public void Apply_ComIFormFile_CriaRequestBodyMultipart()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("file", typeof(IFormFile))]);

        filter.Apply(operation, context);

        Assert.NotNull(operation.RequestBody);
        Assert.True(operation.RequestBody.Content.ContainsKey("multipart/form-data"));
    }

    [Fact(DisplayName = "Apply - Deve criar RequestBody multipart/form-data para IFormFile[]")]
    [Trait("Api", "")]
    public void Apply_ComIFormFileArray_CriaRequestBodyMultipart()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("files", typeof(IFormFile[]))]);

        filter.Apply(operation, context);

        Assert.NotNull(operation.RequestBody);
        Assert.True(operation.RequestBody.Content.ContainsKey("multipart/form-data"));
    }

    [Fact(DisplayName = "Apply - Deve mapear propriedade com formato binary no schema")]
    [Trait("Api", "")]
    public void Apply_ComIFormFile_MapeiaPropiedadeComFormatoBinary()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("arquivo", typeof(IFormFile))]);

        filter.Apply(operation, context);

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
        Assert.True(schema.Properties.ContainsKey("arquivo"));
        Assert.Equal("string", schema.Properties["arquivo"].Type);
        Assert.Equal("binary", schema.Properties["arquivo"].Format);
    }

    [Fact(DisplayName = "Apply - Deve incluir parâmetro obrigatório no conjunto Required")]
    [Trait("Api", "")]
    public void Apply_ComIFormFileObrigatorio_IncluiNoRequired()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("arquivo", typeof(IFormFile), isRequired: true)]);

        filter.Apply(operation, context);

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
        Assert.Contains("arquivo", schema.Required);
    }

    [Fact(DisplayName = "Apply - Não deve incluir parâmetro não-obrigatório no conjunto Required")]
    [Trait("Api", "")]
    public void Apply_ComIFormFileNaoObrigatorio_NaoIncluiNoRequired()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("arquivo", typeof(IFormFile), isRequired: false)]);

        filter.Apply(operation, context);

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
        Assert.DoesNotContain("arquivo", schema.Required);
    }

    [Fact(DisplayName = "Apply - Deve remover parâmetro de arquivo da lista de parâmetros")]
    [Trait("Api", "")]
    public void Apply_ComIFormFile_RemoveParametroDaLista()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters = [new OpenApiParameter { Name = "arquivo" }]
        };
        var context = CreateContext([CreateFileParameter("arquivo", typeof(IFormFile))]);

        filter.Apply(operation, context);

        Assert.DoesNotContain(operation.Parameters, p => p.Name == "arquivo");
    }

    [Fact(DisplayName = "Apply - Deve suportar múltiplos parâmetros de arquivo")]
    [Trait("Api", "")]
    public void Apply_ComMultiplosIFormFiles_CriaMultiplasPropriedades()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([
            CreateFileParameter("foto", typeof(IFormFile)),
            CreateFileParameter("documento", typeof(IFormFile))
        ]);

        filter.Apply(operation, context);

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
        Assert.True(schema.Properties.ContainsKey("foto"));
        Assert.True(schema.Properties.ContainsKey("documento"));
    }

    [Fact(DisplayName = "Apply - Deve manter parâmetros não-arquivo na lista de parâmetros")]
    [Trait("Api", "")]
    public void Apply_ComParametroMistoArquivoENaoArquivo_MantemParametroNaoArquivo()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter { Name = "arquivo" },
                new OpenApiParameter { Name = "descricao" }
            ]
        };
        var context = CreateContext([
            CreateFileParameter("arquivo", typeof(IFormFile)),
            CreateNonFileParameter("descricao")
        ]);

        filter.Apply(operation, context);

        Assert.DoesNotContain(operation.Parameters, p => p.Name == "arquivo");
        Assert.Contains(operation.Parameters, p => p.Name == "descricao");
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "Apply - Não deve lançar exceção quando parâmetro de arquivo não está na lista de parâmetros da operação")]
    [Trait("Api", "")]
    public void Apply_ComIFormFileSemParametroNaOperacao_NaoLancaExcecao()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([CreateFileParameter("arquivo", typeof(IFormFile))]);

        var exception = Record.Exception(() => filter.Apply(operation, context));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Apply - Não deve lançar exceção quando não há parâmetros na ApiDescription")]
    [Trait("Api", "")]
    public void Apply_SemParametros_NaoLancaExcecao()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var context = CreateContext([]);

        var exception = Record.Exception(() => filter.Apply(operation, context));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "Apply - Não deve criar RequestBody quando todos os parâmetros são não-arquivo")]
    [Trait("Api", "")]
    public void Apply_TodosParametrosNaoArquivo_NaoAlteraRequestBody()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters = [new OpenApiParameter { Name = "nome" }]
        };
        var context = CreateContext([CreateNonFileParameter("nome")]);

        filter.Apply(operation, context);

        Assert.Null(operation.RequestBody);
        Assert.Contains(operation.Parameters, p => p.Name == "nome");
    }

    [Fact(DisplayName = "Apply - Não deve lançar exceção quando parâmetro tem ModelMetadata nulo")]
    [Trait("Api", "")]
    public void Apply_ComModelMetadataNulo_NaoLancaExcecao()
    {
        var filter = new FileUploadOperationFilter();
        var operation = new OpenApiOperation { Parameters = [] };
        var param = new ApiParameterDescription { Name = "arquivo", ModelMetadata = null };
        var context = CreateContext([param]);

        var exception = Record.Exception(() => filter.Apply(operation, context));

        Assert.Null(exception);
        Assert.Null(operation.RequestBody);
    }

    #endregion
}
