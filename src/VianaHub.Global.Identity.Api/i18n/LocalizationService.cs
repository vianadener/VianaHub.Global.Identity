using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Serilog;
using System.Globalization;
using System.Text.Json;

namespace VianaHub.Global.Identity.Api.i18n;

/// <summary>
/// Implementação do serviço de localização que carrega traduções de arquivos JSON únicos por idioma.
/// Padrão simplificado: common.{culture}.json (ex: common.pt-PT.json, common.en-US.json, common.es-ES.json)
/// Para adicionar um novo idioma, basta criar um arquivo common.{locale}.json na pasta Localization.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
    private static readonly object _lock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetMessage(string key)
    {
        var culture = GetCurrentCulture();

        var messages = GetMessages(culture);

        if (messages.TryGetValue(key, out var value))
            return value;

        // Fallback para pt-PT
        if (culture != "pt-PT")
        {
            var fallbackMessages = GetMessages("pt-PT");
            if (fallbackMessages.TryGetValue(key, out var fallbackValue))
                return fallbackValue;
        }
        return key;
    }

    public string GetMessage(string key, params object[] args)
    {
        var message = GetMessage(key);
        try
        {
            return string.Format(message, args);
        }
        catch
        {
            return message;
        }
    }

    private string GetCurrentCulture()
    {
        // First, try to get culture from HttpContext.Items (set by RequestLocalizationMiddleware)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue("RequestCulture", out var cultureFromContext) == true
            && cultureFromContext is string cultureName)
        {
            return cultureName;
        }

        // Fallback to CurrentUICulture
        var fallbackCulture = CultureInfo.CurrentUICulture.Name;
        return fallbackCulture;
    }

    private Dictionary<string, string> GetMessages(string culture)
    {
        if (_cache.TryGetValue(culture, out var cached))
            return cached;

        lock (_lock)
        {
            // Double check após lock
            if (_cache.TryGetValue(culture, out cached))
                return cached;

            var localizationPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Localization"
            );

            if (!Directory.Exists(localizationPath))
            {
                Log.Error("❌ [LocalizationService] Localization folder not found: {Path}", localizationPath);
                _cache[culture] = new Dictionary<string, string>();
                return _cache[culture];
            }

            // Padrão simplificado: common.{culture}.json
            // Ex: common.pt-PT.json, common.en-US.json, common.es-ES.json
            var commonFile = Path.Combine(localizationPath, $"common.{culture}.json");

            if (!File.Exists(commonFile))
            {
                Log.Warning("⚠️ [LocalizationService] Common file not found for culture {Culture}: {File}", culture, commonFile);

                // Tentar fallback para pt-PT
                if (culture != "pt-PT")
                {
                    Log.Warning("⚠️ [LocalizationService] Trying pt-PT fallback");
                    commonFile = Path.Combine(localizationPath, "common.pt-PT.json");
                }

                if (!File.Exists(commonFile))
                {
                    Log.Error("❌ [LocalizationService] No fallback file found for culture {Culture}", culture);
                    _cache[culture] = new Dictionary<string, string>();
                    return _cache[culture];
                }
            }

            try
            {
                var json = File.ReadAllText(commonFile);
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);

                if (messages is null || messages.Count == 0)
                {
                    Log.Warning("⚠️ [LocalizationService] File {File} is empty or invalid", Path.GetFileName(commonFile));
                    _cache[culture] = new Dictionary<string, string>();
                    return _cache[culture];
                }

                _cache[culture] = messages;
                Log.Information("✅ [LocalizationService] Successfully loaded {Count} messages for culture {Culture} from {File}",
                    messages.Count, culture, Path.GetFileName(commonFile));

                return messages;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ [LocalizationService] Error loading file {File}", Path.GetFileName(commonFile));
                _cache[culture] = new Dictionary<string, string>();
                return _cache[culture];
            }
        }
    }
}
