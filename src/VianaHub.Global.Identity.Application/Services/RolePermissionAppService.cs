using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using VianaHub.Global.Identity.Application.Dto.Request.RolePermission;
using VianaHub.Global.Identity.Application.Dto.Response.RolePermission;
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

public class RolePermissionAppService : IRolePermissionAppService
{
    private readonly IRolePermissionDomainService _domain;
    private readonly IRolePermissionDataRepository _repository;
    private readonly IMapper _mapper;
    private readonly INotify _notify;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileValidationService _fileValidation;
    private int TenantId { get; set; }
    private int AppId { get; set; }

    public RolePermissionAppService(
        IRolePermissionDomainService domain,
        IRolePermissionDataRepository repository,
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
    }

    public async Task<IList<RolePermissionResponse>> GetAllAsync(CancellationToken ct)
    {
        var list = await _repository.GetAllAsync(TenantId, AppId, ct);
        return _mapper.Map<IList<RolePermissionResponse>>(list);
    }
    public async Task<RolePermissionDetailResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.GetById.Gone"), 410);
            return null;
        }
        return _mapper.Map<RolePermissionDetailResponse>(entity);
    }
    public async Task<ListPage<RolePermissionResponse>> GetPagedAsync(PagedFilter request, CancellationToken ct)
    {
        var pagedList = await _repository.GetPagedAsync(TenantId, AppId, request, ct);
        return _mapper.Map<ListPage<RolePermissionResponse>>(pagedList);
    }

    public async Task<RolePermissionResponse> CreateAsync(CreateRolePermissionRequest request, CancellationToken ct)
    {
        // Valida duplicidade
        var exists = await _repository.ExistsAsync(TenantId, AppId, request.RoleId, request.ResourceId, request.ActionId, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.Create.Exists"), 400);
            return null;
        }

        var entity = new RolePermissionEntity(TenantId, AppId, request.RoleId, request.ResourceId, request.ActionId);
        
        return _mapper.Map<RolePermissionResponse>(await _domain.CreateAsync(entity, ct));
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.Delete.ResourceNotFound"), 410);
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
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.BulkUpload.EmptyFile"), 400);
            return false;
        }

        // Processa cada item
        return await ProcessBulkItemsAsync(items, ct);
    }

    private List<BulkUploadRolePermissionItem> ReadCsvFile(IFormFile file)
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
            var records = new List<BulkUploadRolePermissionItem>();

            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            int maxRows = DomainExtensions.GetMaxCsvRows();

            while (csv.Read() && rowCount < maxRows)
            {
                try
                {
                    var record = csv.GetRecord<BulkUploadRolePermissionItem>();
                    if (record != null)
                    {
                        // Valida se os campos não contêm conteúdo perigoso
                        if (record.AppId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.AppId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }
                        if (record.RoleId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.RoleId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }
                        if (record.ResourceId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.ResourceId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }
                        if (record.ActionId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.ActionId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        records.Add(record);
                    }
                    rowCount++;
                }
                catch (CsvHelperException ex)
                {
                    // Log linha com erro mas continua processamento
                    _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.CsvHelperException", rowCount + 2), 400);
                    rowCount++;
                    continue;
                }
            }

            if (rowCount >= maxRows)
            {
                _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.MaxRows", maxRows), 400);
                return null;
            }

            return records;
        }
        catch (Exception ex)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ReadCsvFile.Exception"), 400);
            return null;
        }
    }
    private async Task<bool> ProcessBulkItemsAsync(List<BulkUploadRolePermissionItem> items, CancellationToken ct)
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
            var exists = await _repository.ExistsAsync(TenantId, item.AppId, item.RoleId, item.ResourceId, item.ActionId, ct);
            if (exists)
            {
                _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ProcessBulkItems.ExistsByName"), 400);
                hasErrors = true;
                continue;
            }

            // Cria a entidade
            var entity = new RolePermissionEntity(TenantId, item.AppId, item.RoleId, item.ResourceId, item.ActionId);

            // Tenta criar no domínio
            var success = await _domain.CreateAsync(entity, ct);

            if (!success)
            {
                _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ProcessBulkItems.FailedToCreate"), 400);
                hasErrors = true;
            }
        }

        return !hasErrors;
    }
    private bool ValidateBulkItem(BulkUploadRolePermissionItem item)
    {
        if (item.AppId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ValidateBulkItem.AppId", item.AppId), 400);
            return false;
        }

        if (item.RoleId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ValidateBulkItem.RoleId", item.RoleId), 400);
            return false;
        }

        if (item.ResourceId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ValidateBulkItem.ResourceId", item.ResourceId), 400);
            return false;
        }

        if (item.ActionId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.RolePermission.ValidateBulkItem.ActionId", item.ActionId), 400);
            return false;
        }

        return true;
    }
}