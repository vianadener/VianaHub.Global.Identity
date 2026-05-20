using VianaHub.Global.Identity.Api.Endpoints.Base;
using Serilog;
using System.Reflection;

namespace VianaHub.Global.Identity.Api.Configuration;

/// <summary>
/// Extensões para registro automático de endpoints via reflexão.
/// </summary>
public static class EndpointMapperExtensions
{
    /// <summary>
    /// Descobre e mapeia automaticamente todos os endpoints da aplicação
    /// que possuem o atributo [EndpointMapper] através de reflexão.
    /// </summary>
    /// <param name="app">WebApplication instance</param>
    /// <returns>WebApplication para encadeamento</returns>
    public static WebApplication MapEndpointsFromAssembly(this WebApplication app)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Busca todos os tipos que possuem o atributo [EndpointMapper]
        var endpointMapperTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && // Classes estáticas são abstract sealed
                        t.GetCustomAttribute<EndpointMapperAttribute>() != null)
            .ToList();

        Log.Information("Descobertos {Count} endpoints para registro automático", endpointMapperTypes.Count);

        foreach (var endpointType in endpointMapperTypes)
        {
            var attribute = endpointType.GetCustomAttribute<EndpointMapperAttribute>();

            // Lógica de geração do nome do método:
            // - Se MethodName está definido no atributo, usa ele
            // - Caso contrário, converte o nome da classe para o padrão do método
            // Exemplos:
            //   ActionEndpoint   -> MapActionEndpoints   (singular -> plural)
            //   AuthEndpoint     -> MapAuthEndpoints     (singular -> plural)
            //   JwtKeyEndpoint   -> MapJwtKeyEndpoints   (singular -> plural)
            string expectedMethodName;
            if (!string.IsNullOrEmpty(attribute?.MethodName))
            {
                expectedMethodName = attribute.MethodName;
            }
            else
            {
                var className = endpointType.Name;

                // Se já termina com "Endpoints" (plural), apenas adiciona prefixo "Map"
                if (className.EndsWith("Endpoints"))
                {
                    expectedMethodName = $"Map{className}";
                }
                // Se termina com "Endpoint" (singular), adiciona "s" e prefixo "Map"
                else if (className.EndsWith("Endpoint"))
                {
                    expectedMethodName = $"Map{className}s";
                }
                // Fallback: adiciona "Endpoints" ao final
                else
                {
                    expectedMethodName = $"Map{className}Endpoints";
                }
            }

            var mapMethod = endpointType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == expectedMethodName &&
                    m.GetParameters().Length > 0 &&
                    (m.GetParameters()[0].ParameterType == typeof(IEndpointRouteBuilder) ||
                     m.GetParameters()[0].ParameterType == typeof(WebApplication)));

            if (mapMethod != null)
            {
                try
                {
                    // Invoca o método de mapeamento
                    mapMethod.Invoke(null, new object[] { app });
                    Log.Debug("Endpoints registrados: {ClassName}.{MethodName}", endpointType.Name, expectedMethodName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Falha ao registrar endpoints para {ClassName}. Método: {MethodName}",
                        endpointType.Name, expectedMethodName);
                    throw new InvalidOperationException(
                        $"Falha ao registrar endpoints para {endpointType.Name}. Método: {expectedMethodName}",
                        ex);
                }
            }
            else
            {
                Log.Warning("Método {MethodName} não encontrado em {ClassName}", expectedMethodName, endpointType.Name);
            }
        }

        return app;
    }
}
