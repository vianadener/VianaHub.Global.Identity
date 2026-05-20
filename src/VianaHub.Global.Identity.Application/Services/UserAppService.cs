using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Application.Dto.Response.User;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace VianaHub.Global.Identity.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserDataRepository _repo;
    private readonly IUserDomainService _domain;
    private readonly ICurrentUserService _currentUser;
    private readonly INotify _notify;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localization;
    private readonly IFileValidationService _fileValidation;
    private int TenantId { get; set; }
    private int UserId { get; set; }

    public UserAppService(
        IUserDataRepository repo,
        IUserDomainService domain,
        INotify notify,
        IMapper mapper,
        ICurrentUserService currentUser,
        ILocalizationService localization,
        IFileValidationService fileValidation)
    {
        _repo = repo;
        _domain = domain;
        _notify = notify;
        _mapper = mapper;
        _currentUser = currentUser;
        _localization = localization;
        _fileValidation = fileValidation;
        TenantId = _currentUser.GetTenantId();
        UserId = _currentUser.GetUserId();
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken ct)
    {
        var entities = await _repo.GetAllAsync(TenantId, ct);
        return _mapper.Map<IEnumerable<UserResponse>>(entities);
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.GetById.Gone"), 410);
            return null;
        }
        return _mapper.Map<UserResponse>(entity);
    }

    public async Task<ListPageResponse<UserResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct)
    {
        var filter = new PagedFilter(request.Search, request.IsActive, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);
        var paged = await _repo.GetPagedAsync(TenantId, filter, ct);
        return _mapper.Map<ListPageResponse<UserResponse>>(paged);
    }

    public async Task<bool> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var exists = await _repo.ExistsByNameAsync(TenantId, request.Name, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.Create.ResourceAlreadyExists"), 400);
            return false;
        }

        var passwordHash = DomainExtensions.HashClientSecret(request.Secret);
        var entity = new UserEntity(TenantId, request.Name, request.Name, passwordHash, request.UrlImage, UserId);

        return await _domain.CreateAsync(entity, ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.Update.ResourceNotFound"), 410);
            return false;
        }

        entity.Update(request.Name, request.UrlImage, UserId);
        return await _domain.UpdateAsync(entity, ct);
    }

    public async Task<bool> UpdatePasswordAsync(int id, UpdateSecretRequest request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.UpdatePassword.ResourceNotFound"), 410);
            return false;
        }

        // Valida a senha atual usando PBKDF2
        if (!DomainExtensions.VerifyClientSecret(entity.PasswordHash, request.CurrentSecret))
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.UpdatePassword.InvalidCurrentPassword"), 400);
            return false;
        }

        // A validação de senha forte já é feita no validador de rota (UpdatePasswordRouteValidator)
        var newPasswordHash = DomainExtensions.HashClientSecret(request.NewSecret);
        entity.UpdatePassword(newPasswordHash, UserId);

        return await _domain.UpdateAsync(entity, ct);
    }

    public async Task<bool> ActivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.Activate.ResourceNotFound"), 410);
            return false;
        }

        entity.Activate(UserId);
        return await _domain.ActivateAsync(entity, ct);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.Deactivate.ResourceNotFound"), 410);
            return false;
        }

        entity.Deactivate(UserId);
        return await _domain.DeactivateAsync(entity, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.Delete.ResourceNotFound"), 410);
            return false;
        }

        entity.Delete(UserId);
        return await _domain.DeleteAsync(entity, ct);
    }

    public async Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct)
    {
        // Valida arquivo usando serviço centralizado
        if (!_fileValidation.ValidateFile(file))
            return false;

        // Lê itens do CSV
        var items = ReadCsvFile(file);
        if (items == null)
            return false;

        if (!items.Any())
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.BulkUpload.EmptyFile"), 400);
            return false;
        }

        // Processa cada item
        return await ProcessBulkItemsAsync(items, ct);
    }

    private List<BulkUploadUserItem> ReadCsvFile(IFormFile file)
    {
        try
        {
            // Cria StreamReader com encoding UTF-8 forçado
            using var reader = file.OpenReadStream().CreateUtf8StreamReader();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";", // CSV usa ponto e vírgula como delimitador
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null // Ignora linhas mal formatadas ao invés de lançar exceção
            };

            using var csv = new CsvReader(reader, config);
            var records = new List<BulkUploadUserItem>();

            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            int maxRows = DomainExtensions.GetMaxCsvRows();

            while (csv.Read() && rowCount < maxRows)
            {
                try
                {
                    var record = csv.GetRecord<BulkUploadUserItem>();
                    if (record != null)
                    {
                        // Sanitiza e normaliza campos
                        record.Name = record.Name?.SanitizeCsvInput().NormalizeUtf8();
                        record.Secret = record.Secret?.SanitizeCsvInput().NormalizeUtf8();

                        // Valida se os campos não contêm conteúdo perigoso
                        if (!string.IsNullOrEmpty(record.Name) && !record.Name.IsSafeCsvValue())
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.User.ReadCsvFile.Name.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        records.Add(record);
                    }
                    rowCount++;
                }
                catch (CsvHelperException ex)
                {
                    // Log linha com erro mas continua processamento
                    _notify.Add(_localization.GetMessage("Application.Service.User.ReadCsvFile.CsvHelperException", rowCount + 2), 400);
                    rowCount++;
                    continue;
                }
            }

            if (rowCount >= maxRows)
            {
                _notify.Add(_localization.GetMessage("Application.Service.User.ReadCsvFile.MaxRows", maxRows), 400);
                return null;
            }

            return records;
        }
        catch (Exception ex)
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.ReadCsvFile.Exception"), 400);
            return null;
        }
    }

    private async Task<bool> ProcessBulkItemsAsync(List<BulkUploadUserItem> items, CancellationToken ct)
    {
        var hasErrors = false;

        foreach (var item in items)
        {
            // Valida campos obrigatórios
            if (!ValidateBulkItem(item))
            {
                hasErrors = true;
                continue;
            }

            // Verifica duplicidade
            var exists = await _repo.ExistsByNameAsync(TenantId, item.Name, ct);
            if (exists)
            {
                _notify.Add(_localization.GetMessage("Application.Service.User.ProcessBulkItems.ExistsByEmail", item.Name), 400);
                hasErrors = true;
                continue;
            }

            // Cria a entidade usando hash PBKDF2
            var secretHash = DomainExtensions.HashClientSecret(item.Secret);
            var entity = new UserEntity(TenantId, item.Name, item.Secret, secretHash, item.UrlImage, UserId);

            // Tenta criar no domínio
            var success = await _domain.CreateAsync(entity, ct);

            if (!success)
            {
                _notify.Add(_localization.GetMessage("Application.Service.User.ProcessBulkItems.FailedToCreate", item.Name), 400);
                hasErrors = true;
            }
        }

        return !hasErrors;
    }

    private bool ValidateBulkItem(BulkUploadUserItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.ValidateBulkItem.Name", item.Name ?? ""), 400);
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.Secret))
        {
            _notify.Add(_localization.GetMessage("Application.Service.User.ValidateBulkItem.Secret", item.Secret), 400);
            return false;
        }

        return true;
    }
}
