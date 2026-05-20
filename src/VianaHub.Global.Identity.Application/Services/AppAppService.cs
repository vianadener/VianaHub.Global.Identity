using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.App;
using VianaHub.Global.Identity.Application.Dto.Response.App;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace VianaHub.Global.Identity.Application.Services;

public class AppAppService : IAppAppService
{
    private readonly IAppDataRepository _repo;
    private readonly IAppDomainService _domain;
    private readonly ICurrentUserService _currentUser;
    private readonly INotify _notify;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localization;
    private readonly IFileValidationService _fileValidation;

    public AppAppService(
        IAppDataRepository repo,
        IAppDomainService domain,
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
    }

    public async Task<IEnumerable<AppResponse>> GetAllAsync(CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();
        var entities = await _repo.GetAllAsync(tenantId, ct);
        return _mapper.Map<IEnumerable<AppResponse>>(entities);
    }

    public async Task<AppResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();
        var entity = await _repo.GetByIdAsync(tenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.GetById.Gone"), 410);
            return null;
        }
        return _mapper.Map<AppResponse>(entity);
    }

    public async Task<ListPageResponse<AppResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();
        var filter = new PagedFilter(request.Search, request.IsActive, request.PageNumber, request.PageSize, request.SortBy, request.SortDirection);
        var paged = await _repo.GetPagedAsync(tenantId, filter, ct);
        return _mapper.Map<ListPageResponse<AppResponse>>(paged);
    }

    public async Task<bool> CreateAsync(CreateAppRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var exists = await _repo.ExistsByNameAsync(tenantId, request.Name, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.Create.ResourceAlreadyExists"), 400);
            return false;
        }

        var entity = new AppEntity(tenantId, request.Name, request.Description, _currentUser.GetUserId());
        return await _domain.CreateAsync(entity, ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateAppRequest request, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var entity = await _repo.GetByIdAsync(tenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.Update.ResourceNotFound"), 410);
            return false;
        }

        entity.Update(request.Name, request.Description, _currentUser.GetUserId());
        return await _domain.UpdateAsync(entity, ct);
    }

    public async Task<bool> ActivateAsync(int id, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var entity = await _repo.GetByIdAsync(tenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.Activate.ResourceNotFound"), 410);
            return false;
        }

        entity.Activate(_currentUser.GetUserId());
        return await _domain.ActivateAsync(entity, ct);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var entity = await _repo.GetByIdAsync(tenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.Deactivate.ResourceNotFound"), 410);
            return false;
        }

        entity.Deactivate(_currentUser.GetUserId());
        return await _domain.DeactivateAsync(entity, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();

        var entity = await _repo.GetByIdAsync(tenantId, id, ct);
        if (entity == null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.Delete.ResourceNotFound"), 410);
            return false;
        }

        entity.Delete(_currentUser.GetUserId());
        return await _domain.DeleteAsync(entity, ct);
    }

    public async Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct)
    {
        if (!_fileValidation.ValidateFile(file))
            return false;

        var items = ReadCsvFile(file);
        if (items == null)
            return false;

        if (!items.Any())
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.BulkUpload.EmptyFile"), 400);
            return false;
        }

        return await ProcessBulkItemsAsync(items, ct);
    }

    private List<BulkUploadAppItem> ReadCsvFile(IFormFile file)
    {
        try
        {
            using var reader = file.OpenReadStream().CreateUtf8StreamReader();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            };

            using var csv = new CsvReader(reader, config);
            var records = new List<BulkUploadAppItem>();

            csv.Read();
            csv.ReadHeader();

            int rowCount = 0;
            int maxRows = DomainExtensions.GetMaxCsvRows();

            while (csv.Read() && rowCount < maxRows)
            {
                try
                {
                    var record = csv.GetRecord<BulkUploadAppItem>();
                    if (record != null)
                    {
                        record.Name = record.Name?.SanitizeCsvInput().NormalizeUtf8();
                        record.Description = record.Description?.SanitizeCsvInput().NormalizeUtf8();

                        if (!string.IsNullOrEmpty(record.Name) && !record.Name.IsSafeCsvValue())
                        {
                            _notify.Add(_localization.GetMessage("Application.Service.App.ReadCsvFile.Name.IsSafeCsvValue", rowCount + 2), 400);
                            continue;
                        }

                        records.Add(record);
                    }
                    rowCount++;
                }
                catch (CsvHelperException)
                {
                    _notify.Add(_localization.GetMessage("Application.Service.App.ReadCsvFile.CsvHelperException", rowCount + 2), 400);
                    rowCount++;
                    continue;
                }
            }

            if (rowCount >= maxRows)
            {
                _notify.Add(_localization.GetMessage("Application.Service.App.ReadCsvFile.MaxRows", maxRows), 400);
                return null;
            }

            return records;
        }
        catch (Exception)
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.ReadCsvFile.Exception"), 400);
            return null;
        }
    }

    private async Task<bool> ProcessBulkItemsAsync(List<BulkUploadAppItem> items, CancellationToken ct)
    {
        var tenantId = _currentUser.GetTenantId();
        var hasErrors = false;

        foreach (var item in items)
        {
            if (!ValidateBulkItem(item))
            {
                hasErrors = true;
                continue;
            }

            var exists = await _repo.ExistsByNameAsync(tenantId, item.Name, ct);
            if (exists)
            {
                _notify.Add(_localization.GetMessage("Application.Service.App.ProcessBulkItems.ExistsByName", item.Name), 400);
                hasErrors = true;
                continue;
            }

            var entity = new AppEntity(tenantId, item.Name, item.Description, _currentUser.GetUserId());

            var success = await _domain.CreateAsync(entity, ct);
            if (!success)
            {
                hasErrors = true;
            }
        }

        return !hasErrors;
    }

    private bool ValidateBulkItem(BulkUploadAppItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.ValidateBulkItem.NameRequired"), 400);
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            _notify.Add(_localization.GetMessage("Application.Service.App.ValidateBulkItem.DescriptionRequired"), 400);
            return false;
        }

        return true;
    }
}
