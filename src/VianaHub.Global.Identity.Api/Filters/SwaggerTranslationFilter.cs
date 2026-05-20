using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Text.Json;

namespace VianaHub.Global.Identity.Api.Filters;

/// <summary>
/// Document filter que traduz a documentação Swagger/OpenAPI baseado na cultura atual.
/// Carrega todos os arquivos de localização presentes na pasta `Localization` recursivamente e mescla as chaves para a cultura solicitada.
/// </summary>
public class SwaggerTranslationFilter : IDocumentFilter
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translationsCache = new();
    private static readonly object _lock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SwaggerTranslationFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        try
        {
            // Tenta pegar a cultura do HttpContext.Items (definida pelo SwaggerLocalizationMiddleware)
            var culture = CultureInfo.CurrentUICulture.Name;

            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("SwaggerCulture", out var swaggerCulture) == true)
            {
                culture = swaggerCulture?.ToString() ?? culture;
                Log.Information("✅ [SwaggerTranslation] Using culture from HttpContext.Items: {Culture}", culture);
            }
            else
            {
                Log.Debug("🔍 [SwaggerTranslation] No culture in HttpContext.Items, using CurrentUICulture: {Culture}", culture);
            }

            Log.Information("📄 [SwaggerTranslation] Applying translations for culture: {Culture}", culture);

            // Carrega as traduções mescladas de todos os arquivos JSON na pasta Localization
            var translations = LoadTranslations(culture);
            if (translations == null || translations.Count == 0)
            {
                Log.Warning("?? [SwaggerTranslation] No translations found for culture: {Culture}", culture);
                return;
            }

            // Traduz as informações da API
            TranslateApiInfo(swaggerDoc.Info, translations);

            // Traduz todos os paths (endpoints)
            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var operation in path.Value.Operations.Values)
                {
                    TranslateOperation(operation, translations);
                }
            }

            // Traduz schemas
            if (swaggerDoc.Components?.Schemas != null)
            {
                foreach (var schema in swaggerDoc.Components.Schemas.Values)
                {
                    TranslateSchema(schema, translations);
                }
            }

            // Traduz security schemes
            if (swaggerDoc.Components?.SecuritySchemes != null)
            {
                foreach (var securityScheme in swaggerDoc.Components.SecuritySchemes.Values)
                {
                    TranslateSecurityScheme(securityScheme, translations);
                }
            }

            Log.Information("? [SwaggerTranslation] Successfully translated Swagger document to {Culture}", culture);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "? [SwaggerTranslation] Error translating Swagger document");
        }
    }

    /// <summary>
    /// Carrega as traduções procurando recursivamente por arquivos '*.{culture}.json' dentro da pasta 'Localization' e mescla-os.
    /// Utiliza cache por cultura.
    /// </summary>
    private Dictionary<string, string>? LoadTranslations(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            culture = CultureInfo.CurrentUICulture.Name;

        if (_translationsCache.TryGetValue(culture, out var cached))
        {
            return cached;
        }

        lock (_lock)
        {
            if (_translationsCache.TryGetValue(culture, out cached))
                return cached;

            try
            {
                // Use AppContext.BaseDirectory para garantir que localizamos os arquivos na pasta de saída
                var basePath = AppContext.BaseDirectory;
                var localizationPath = Path.Combine(basePath, "i18n");

                if (!Directory.Exists(localizationPath))
                {
                    Log.Warning("?? [SwaggerTranslation] i18n folder not found: {Path}", localizationPath);
                    _translationsCache[culture] = new Dictionary<string, string>();
                    return _translationsCache[culture];
                }

                var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var duplicateKeys = new List<string>();

                // Tentar cadeia de culturas: ex: "pt-PT", "pt", etc.
                var tryCultures = new[] {
                    culture,
                    // Surround with try/catch for segurança caso a cultura seja inválida
                    CultureInfo.GetCultureInfo(culture).Name,
                    CultureInfo.GetCultureInfo(culture).TwoLetterISOLanguageName
                }.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                foreach (var c in tryCultures)
                {
                    // Buscar arquivos JSON dentro da pasta da cultura (ex: i18n/pt-PT/*.json)
                    var culturePath = Path.Combine(localizationPath, c);
                    if (!Directory.Exists(culturePath))
                    {
                        Log.Debug("?? [SwaggerTranslation] Culture folder not found: {Path}", culturePath);
                        continue;
                    }

                    var files = Directory.GetFiles(culturePath, "*.json", SearchOption.AllDirectories);

                    if (files.Length == 0)
                    {
                        Log.Debug("?? [SwaggerTranslation] No JSON files found in {Path}", culturePath);
                        continue;
                    }

                    Log.Debug("?? [SwaggerTranslation] Found {Count} files for culture {CultureToken}", files.Length, c);

                    foreach (var file in files)
                    {
                        try
                        {
                            var json = File.ReadAllText(file);
                            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                            if (dict == null || dict.Count == 0)
                            {
                                Log.Debug("?? [SwaggerTranslation] File {File} is empty or invalid", Path.GetFileName(file));
                                continue;
                            }

                            foreach (var kvp in dict)
                            {
                                if (merged.ContainsKey(kvp.Key))
                                {
                                    duplicateKeys.Add(kvp.Key);
                                    // Não sobrescrever, manter primeira ocorrência (prioridade por ordem de descoberta)
                                }
                                else
                                {
                                    merged[kvp.Key] = kvp.Value;
                                }
                            }

                            Log.Debug("? [SwaggerTranslation] Loaded {Count} messages from {File}", dict.Count, Path.GetFileName(file));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "? [SwaggerTranslation] Error loading translation file {File}", file);
                        }
                    }

                    // Se já carregou chaves para a cultura exata, podemos parar antes de tentar a versão de duas letras
                    if (merged.Count > 0)
                        break;
                }

                if (merged.Count == 0)
                {
                    // Tentar fallback en-US (padrão da aplicação)
                    if (!culture.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning("?? [SwaggerTranslation] No translations for {Culture}, trying fallback en-US", culture);
                        var fallback = LoadTranslations("en-US");
                        _translationsCache[culture] = fallback ?? new Dictionary<string, string>();
                        return _translationsCache[culture];
                    }

                    Log.Warning("?? [SwaggerTranslation] No translation files found for culture chain: {Culture}", culture);
                    _translationsCache[culture] = new Dictionary<string, string>();
                    return _translationsCache[culture];
                }

                if (duplicateKeys.Count > 0)
                {
                    Log.Warning("?? [SwaggerTranslation] Found {Count} duplicate keys while merging translations: {Keys}", duplicateKeys.Count, string.Join(", ", duplicateKeys.Distinct()));
                }

                _translationsCache[culture] = merged;
                Log.Information("? [SwaggerTranslation] Successfully loaded {Count} translations for culture {Culture}", merged.Count, culture);
                return merged;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "? [SwaggerTranslation] Error loading translations for culture: {Culture}", culture);
                _translationsCache[culture] = new Dictionary<string, string>();
                return _translationsCache[culture];
            }
        }
    }

    private void TranslateApiInfo(OpenApiInfo info, Dictionary<string, string> translations)
    {
        if (info == null) return;

        // Se o Title/Description já é uma chave (ex: "Swagger.Api.Title.Development"), usa diretamente
        // Caso contrário, tenta traduzir normalmente
        if (!string.IsNullOrEmpty(info.Title) && info.Title.StartsWith("Swagger."))
        {
            info.Title = GetTranslation(translations, info.Title, info.Title);
        }

        if (!string.IsNullOrEmpty(info.Description) && info.Description.StartsWith("Swagger."))
        {
            info.Description = GetTranslation(translations, info.Description, info.Description);
        }

        if (info.Contact != null && !string.IsNullOrEmpty(info.Contact.Name) && info.Contact.Name.StartsWith("Swagger."))
        {
            info.Contact.Name = GetTranslation(translations, info.Contact.Name, info.Contact.Name);
        }

        if (info.License != null && !string.IsNullOrEmpty(info.License.Name) && info.License.Name.StartsWith("Swagger."))
        {
            info.License.Name = GetTranslation(translations, info.License.Name, info.License.Name);
        }
    }

    private void TranslateOperation(OpenApiOperation operation, Dictionary<string, string> translations)
    {
        if (operation == null) return;

        // Traduz o Summary se começar com "Swagger."
        if (!string.IsNullOrEmpty(operation.Summary) && operation.Summary.StartsWith("Swagger."))
        {
            operation.Summary = GetTranslation(translations, operation.Summary, operation.Summary);
        }

        // Traduz o OperationId
        operation.Summary = GetTranslation(translations, $"Swagger.Endpoint.{operation.OperationId}.Summary", operation.Summary);
        operation.Description = GetTranslation(translations, $"Swagger.Endpoint.{operation.OperationId}.Description", operation.Description);

        // Traduz parâmetros
        if (operation.Parameters != null)
        {
            foreach (var param in operation.Parameters)
            {
                param.Description = GetTranslation(translations, $"Swagger.Parameter.{param.Name}.Description", param.Description);
            }
        }

        // Traduz respostas
        if (operation.Responses != null)
        {
            foreach (var response in operation.Responses)
            {
                response.Value.Description = GetTranslation(translations, $"Swagger.Response.{response.Key}.Description", response.Value.Description);
            }
        }

        // Traduz tags
        if (operation.Tags != null)
        {
            for (int i = 0; i < operation.Tags.Count; i++)
            {
                var tag = operation.Tags[i];
                var translatedName = GetTranslation(translations, $"Swagger.Tag.{tag.Name}", tag.Name);
                if (translatedName != tag.Name)
                {
                    operation.Tags[i] = new OpenApiTag { Name = translatedName };
                }
            }
        }
    }

    private void TranslateSchema(OpenApiSchema schema, Dictionary<string, string> translations)
    {
        if (schema == null) return;

        // Tenta usar Title, se não existir usa Reference.Id (quando schemas são referenciados)
        var schemaId = schema.Title ?? schema.Reference?.Id;
        if (!string.IsNullOrEmpty(schemaId))
        {
            schema.Description = GetTranslation(translations, $"Swagger.Schema.{schemaId}.Description", schema.Description);
        }

        // Traduz propriedades do schema
        if (schema.Properties != null)
        {
            foreach (var prop in schema.Properties)
            {
                prop.Value.Description = GetTranslation(translations, $"Swagger.Property.{prop.Key}.Description", prop.Value.Description);
            }
        }
    }

    private void TranslateSecurityScheme(OpenApiSecurityScheme securityScheme, Dictionary<string, string> translations)
    {
        if (securityScheme == null) return;

        securityScheme.Description = GetTranslation(translations, "Swagger.Security.Bearer.Description", securityScheme.Description);
    }

    /// <summary>
    /// Obtém a tradução ou retorna o valor padrão se não encontrar
    /// </summary>
    private string GetTranslation(Dictionary<string, string> translations, string key, string? defaultValue)
    {
        if (string.IsNullOrEmpty(key) || translations == null)
        {
            return defaultValue ?? string.Empty;
        }

        if (translations.TryGetValue(key, out var translation) && !string.IsNullOrEmpty(translation))
        {
            return translation;
        }

        return defaultValue ?? string.Empty;
    }
}
