using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Resource;
using VianaHub.Global.Identity.Application.Dto.Response.Resource;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace VianaHub.Global.Identity.Application.Services;

public class ResourceAppService : IResourceAppService
{
    private readonly IResourceDataRepository _repo;
    private readonly IResourceDomainService _domain;
    private readonly ICurrentUserService _currentUser;
    private readonly INotify _notify;
    private readonly IMapper _mapper;
    private readonly ILogger<ResourceAppService> _logger;
    private readonly ILocalizationService _localization;
    private readonly IFileValidationService _fileValidation;
    private int TenantId { get; set; }
    private int AppId { get; set; }
    private int UserId { get; set; }

    public ResourceAppService(
        IResourceDataRepository repo,
        IResourceDomainService domain,
        INotify notify,
        IMapper mapper,
        ILogger<ResourceAppService> logger,
        ICurrentUserService currentUser,
        ILocalizationService localization,
        IFileValidationService fileValidation)
    {
        _repo = repo;
        _domain = domain;
        _notify = notify;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _localization = localization;
        _fileValidation = fileValidation;
        TenantId = _currentUser.GetTenantId();
        AppId = _currentUser.GetAppId();
        UserId = _currentUser.GetUserId();
    }

    public async Task<IEnumerable<ResourceResponse>> GetAllAsync(CancellationToken ct)
    {
        var entities = await _repo.GetAllAsync(TenantId, AppId, ct);

        return _mapper.Map<IEnumerable<ResourceResponse>>(entities);
    }
    public async Task<ResourceResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, AppId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.GetById.Gone"), 410);
            return null;
        }

        return _mapper.Map<ResourceResponse>(entity);
    }
    public async Task<ListPageResponse<ResourceResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct)
    {
        var filter = new PagedFilter(request.Search, request.IsActive, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);
        
        return _mapper.Map<ListPageResponse<ResourceResponse>>(await _repo.GetPagedAsync(TenantId, AppId, filter, ct));
    }

    public async Task<bool> CreateAsync(CreateResourceRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var exists = await _repo.ExistsByNameAsync(tenantId, request.AppId, request.Name, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.Create.ResourceAlreadyExists"), 400);
            return false;
        }

        var entity = new ResourceEntity(tenantId, request.AppId, request.Name, request.Description, UserId);
        return await _domain.CreateAsync(entity, ct);
    }
    public async Task<bool> UpdateAsync(int id, UpdateResourceRequest request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, AppId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.Update.Gone"), 410);
            return false;
        }

        entity.Update(request.Name, request.Description, UserId);
        return await _domain.UpdateAsync(entity, ct);
    }
    public async Task<bool> ActivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, AppId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.Activate.Gone"), 410);
            return false;
        }

        entity.Activate(UserId);
        return await _domain.ActivateAsync(entity, ct);
    }
    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, AppId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.Deactivate.Gone"), 410);
            return false;
        }

        entity.Deactivate(UserId);
        return await _domain.DeactivateAsync(entity, ct);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(TenantId, AppId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.Delete.Gone"), 410);
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
            _notify.Add(_localization.GetMessage("Application.Service.Resource.BulkUpload.EmptyFile"), 400);
            return false;
        }

        // Processa cada item
        return await ProcessBulkItemsAsync(items, ct);
    }


    private List<BulkUploadResourceItem> ReadCsvFile(IFormFile file)
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
            var records = new List<BulkUploadResourceItem>();

            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            int maxRows = DomainExtensions.GetMaxCsvRows();

            while (csv.Read() && rowCount < maxRows)
            {
                try
                {
                    var record = csv.GetRecord<BulkUploadResourceItem>();
                    if (record != null)
                    {
                        // Sanitiza e normaliza campos
                        record.Name = record.Name?.SanitizeCsvInput().NormalizeUtf8();
                        record.Description = record.Description?.SanitizeCsvInput().NormalizeUtf8();

                        // Valida AppId
                        if (record.AppId <= 0)
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.AppId.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        // Valida se os campos não contêm conteúdo perigoso
                        if (!string.IsNullOrEmpty(record.Name) && !record.Name.IsSafeCsvValue())
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.Name.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(record.Description) && !record.Description.IsSafeCsvValue())
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.Description.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        records.Add(record);
                    }
                    rowCount++;
                }
                catch (CsvHelperException ex)
                {
                    // Log linha com erro mas continua processamento
                    _logger.LogWarning(ex, "Erro ao processar linha {RowNumber} do CSV de Resources", rowCount + 2);
                    _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.CsvHelperException", rowCount + 2), 400);
                    rowCount++;
                    continue;
                }
            }

            if (rowCount >= maxRows)
            {
                _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.MaxRows", maxRows), 400);
                return null;
            }

            return records;
        }
        catch (Exception ex)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.ReadCsvFile.Exception"), 400);
            return null;
        }
    }
    private async Task<bool> ProcessBulkItemsAsync(List<BulkUploadResourceItem> items, CancellationToken ct)
    {
        var hasErrors = false;
        var tenantId = _currentUser.GetTenantId();

        foreach (var item in items)
        {
            // Valida campos obrigatórios
            if (!ValidateBulkItem(item))
            {
                hasErrors = true;
                continue;
            }

            // Verifica duplicidade
            var exists = await _repo.ExistsByNameAsync(tenantId, item.AppId, item.Name, ct);
            if (exists)
            {
                _notify.Add(_localization.GetMessage("Application.Service.Resource.ProcessBulkItems.ExistsByName", item.Name), 400);
                hasErrors = true;
                continue;
            }

            // Cria a entidade
            var entity = new ResourceEntity(tenantId, item.AppId, item.Name, item.Description, UserId);

            // Tenta criar no domínio
            var success = await _domain.CreateAsync(entity, ct);

            if (!success)
            {
                _notify.Add(_localization.GetMessage("Application.Service.Resource.ProcessBulkItems.FailedToCreate", item.Name), 400);
                hasErrors = true;
            }
        }

        return !hasErrors;
    }
    private bool ValidateBulkItem(BulkUploadResourceItem item)
    {
        if (item.AppId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.ValidateBulkItem.AppId", item.Name), 400);
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.ValidateBulkItem.Name", item.Name), 400);
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Resource.ValidateBulkItem.Description", item.Name), 400);
            return false;
        }

        return true;
    }
}
