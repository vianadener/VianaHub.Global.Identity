using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Application.Services;

/// <summary>
/// Serviço responsável pela validação de arquivos de upload.
/// Centraliza todas as regras de validação para garantir consistência e reusabilidade em toda a aplicação.
/// Implementa os princípios de Single Responsibility e DRY (Don't Repeat Yourself).
/// </summary>
public class FileValidationService(INotify notify, ILocalizationService localization) : IFileValidationService
{
    private readonly INotify _notify = notify;
    private readonly ILocalizationService _localization = localization;

    /// <summary>
    /// Valida um arquivo de upload baseado em regras de segurança e formato.
    /// </summary>
    /// <param name="file">Arquivo a ser validado.</param>
    /// <returns>True se o arquivo é válido, False caso contrário.</returns>
    public bool ValidateFile(IFormFile file)
    {
        // Valida se o arquivo existe e não está vazio
        if (file is null || file.Length == 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.File.ValidateFile.InvalidFile"), 400);
            return false;
        }

        // Valida tamanho do arquivo
        if (!file.Length.IsValidCsvFileSize())
        {
            _notify.Add(_localization.GetMessage("Application.Service.File.ValidateFile.IsValidCsvFileSize"), 400);
            return false;
        }

        // Valida nome do arquivo (previne path traversal)
        if (!file.FileName.IsSafeCsvFileName())
        {
            _notify.Add(_localization.GetMessage("Application.Service.File.ValidateFile.IsSafeCsvFileName"), 400);
            return false;
        }

        // Valida extensão
        if (!file.FileName.HasValidCsvExtension())
        {
            _notify.Add(_localization.GetMessage("Application.Service.File.ValidateFile.OnlyCsvAllowed"), 400);
            return false;
        }

        // Valida encoding UTF-8
        using var stream = file.OpenReadStream();
        if (!stream.IsValidUtf8Encoding())
        {
            _notify.Add(_localization.GetMessage("Application.Service.File.ValidateFile.InvalidEncoding"), 400);
            return false;
        }

        return true;
    }
}
