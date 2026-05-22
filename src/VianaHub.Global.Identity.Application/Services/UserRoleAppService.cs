using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using VianaHub.Global.Identity.Application.Dto.Request.UserRole;
using VianaHub.Global.Identity.Application.Dto.Response.UserRole;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace VianaHub.Global.Identity.Application.Services;

public class UserRoleAppService : IUserRoleAppService
{
    private readonly IUserRoleDomainService _domain;
    private readonly IUserRoleDataRepository _repository;
    private readonly IMapper _mapper;
    private readonly INotify _notify;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileValidationService _fileValidation;
    private int TenantId { get; set; }
    private int AppId { get; set; }
    private int UserId { get; set; }

    public UserRoleAppService(
        IUserRoleDomainService domain,
        IUserRoleDataRepository repository,
        IMapper mapper,
        INotify notify,
        ILocalizationService localization,
        ICurrentUserService currentUser,
        IFileValidationService fileValidation)
    {
        _domain = domain;
        _repository = repository;
        _mapper = mapper;
        _notify = notify;
        _localization = localization;
        _currentUser = currentUser;
        _fileValidation = fileValidation;
        TenantId = _currentUser.GetTenantId();
        AppId = _currentUser.GetAppId();
        UserId = _currentUser.GetUserId();
    }

    public async Task<IList<UserRoleResponse>> GetAllAsync(CancellationToken ct)
    {
        var list = await _repository.GetAllAsync(TenantId, AppId, ct);
        return _mapper.Map<IList<UserRoleResponse>>(list);
    }
    public async Task<UserRoleResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.GetById.Gone"), 410);
            return null;
        }
        return _mapper.Map<UserRoleResponse>(entity);
    }
    public async Task<ListPage<UserRoleEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct)
    {
        return await _repository.GetPagedAsync(TenantId, AppId, request, ct);
    }

    public async Task<UserRoleResponse> CreateAsync(CreateUserRoleRequest request, CancellationToken ct)
    {
        // Valida se existe
        var exists = await _repository.ExistsAsync(TenantId, request.AppId, request.UserId, request.RoleId, CancellationToken.None);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.Create.Exists"), 404);
            return null;
        }

        var entity = new UserRoleEntity(TenantId, request.AppId, request.UserId, request.RoleId);
        
        return _mapper.Map<UserRoleResponse>(await _domain.CreateAsync(entity, ct));
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            // Not found -> notify with 410 Gone
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.Delete.ResourceNotFound"), 410);
            return;
        }

        await _repository.DeleteAsync(entity, ct);
    }
    public async Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct)
    {
        // Valida arquivo usando serviço centralizado
        if (!_fileValidation.ValidateFile(file))
            return false;

        // Lê itens do CSV
        var items = ReadCsvFile(file);
        if (items is null)
            return false;

        if (!items.Any())
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.BulkUpload.EmptyFile"), 400);
            return false;
        }

        // Processa cada item
        return await ProcessBulkItemsAsync(items, ct);
    }
    private List<BulkUploadUserRoleItem> ReadCsvFile(IFormFile file)
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
            var records = new List<BulkUploadUserRoleItem>();

            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            int maxRows = DomainExtensions.GetMaxCsvRows();

            while (csv.Read() && rowCount < maxRows)
            {
                try
                {
                    var record = csv.GetRecord<BulkUploadUserRoleItem>();
                    if (record != null)
                    {
                        // Valida se os campos não contêm conteúdo perigoso
                        if (record.UserId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.UserRole.ReadCsvFile.UserId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        if (record.RoleId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.UserRole.ReadCsvFile.RoleId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        records.Add(record);
                    }
                    rowCount++;
                }
                catch (CsvHelperException ex)
                {
                    // Log linha com erro mas continua processamento
                    _notify.Add(_localization.GetMessage("Application.Service.UserRole.ReadCsvFile.CsvHelperException", rowCount + 2), 400);
                    rowCount++;
                    continue;
                }
            }

            if (rowCount >= maxRows)
            {
                _notify.Add(_localization.GetMessage("Application.Service.UserRole.ReadCsvFile.MaxRows", maxRows), 400);
                return null;
            }

            return records;
        }
        catch (Exception ex)
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.ReadCsvFile.Exception"), 400);
            return null;
        }
    }
    private async Task<bool> ProcessBulkItemsAsync(List<BulkUploadUserRoleItem> items, CancellationToken ct)
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
            var exists = await _repository.ExistsAsync(TenantId, AppId, item.UserId, item.RoleId, ct);
            if (exists)
            {
                _notify.Add(_localization.GetMessage("Application.Service.UserRole.ProcessBulkItems.ExistsByEmail"), 400);
                hasErrors = true;
                continue;
            }

            var entity = new UserRoleEntity(TenantId, AppId, item.UserId, item.RoleId);

            // Tenta criar no domínio
            var success = await _domain.CreateAsync(entity, ct);

            if (!success)
            {
                _notify.Add(_localization.GetMessage("Application.Service.UserRole.ProcessBulkItems.FailedToCreate"), 400);
                hasErrors = true;
            }
        }

        return !hasErrors;
    }
    private bool ValidateBulkItem(BulkUploadUserRoleItem item)
    {
        if (item.UserId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.ValidateBulkItem.UserId", item.UserId), 400);
            return false;
        }
        if (item.RoleId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.UserRole.ValidateBulkItem.RoleId", item.RoleId), 400);
            return false;
        }

        return true;
    }
}
