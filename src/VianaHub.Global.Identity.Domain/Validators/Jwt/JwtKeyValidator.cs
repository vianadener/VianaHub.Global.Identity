using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation.Results;

namespace VianaHub.Global.Identity.Domain.Validators.Jwt;

/// <summary>
/// Validações de domínio para entidades de chaves JWT.
/// Implementação simples que garante campos essenciais e segue o padrão de retorno via ValidationResult.
/// </summary>
public class JwtKeyValidator : IEntityDomainValidator<JwtKeyEntity>
{
    private readonly ILocalizationService _localization;

    public JwtKeyValidator(ILocalizationService localization)
    {
        _localization = localization;
    }

    public Task<ValidationResult> ValidateForCreateAsync(JwtKeyEntity entity)
    {
        var vr = new ValidationResult();

        if (entity is null)
        {
            vr.Errors.Add(new ValidationFailure("JwtKey", _localization.GetMessage("Domain.JwtKey.EntityRequired")));
            return Task.FromResult(vr);
        }

        if (entity.TenantId <= 0)
            vr.Errors.Add(new ValidationFailure(nameof(entity.TenantId), _localization.GetMessage("Domain.JwtKey.TenantRequired")));

        if (string.IsNullOrWhiteSpace(entity.PublicKey))
            vr.Errors.Add(new ValidationFailure(nameof(entity.PublicKey), _localization.GetMessage("Domain.JwtKey.PublicKeyRequired")));

        if (string.IsNullOrWhiteSpace(entity.PrivateKeyEncrypted))
            vr.Errors.Add(new ValidationFailure(nameof(entity.PrivateKeyEncrypted), _localization.GetMessage("Domain.JwtKey.PrivateKeyRequired")));

        if (string.IsNullOrWhiteSpace(entity.Algorithm))
            vr.Errors.Add(new ValidationFailure(nameof(entity.Algorithm), _localization.GetMessage("Domain.JwtKey.AlgorithmRequired")));

        if (entity.KeySize < 1024)
            vr.Errors.Add(new ValidationFailure(nameof(entity.KeySize), _localization.GetMessage("Domain.JwtKey.KeySizeInvalid")));

        return Task.FromResult(vr);
    }

    public Task<ValidationResult> ValidateForUpdateAsync(JwtKeyEntity entity)
    {
        // Atualização básica: não permite alterar TenantId/KeyId/public key via update
        var vr = new ValidationResult();
        if (entity is null)
        {
            vr.Errors.Add(new ValidationFailure("JwtKey", _localization.GetMessage("Domain.JwtKey.EntityRequired")));
            return Task.FromResult(vr);
        }

        if (entity.TenantId <= 0)
            vr.Errors.Add(new ValidationFailure(nameof(entity.TenantId), _localization.GetMessage("Domain.JwtKey.TenantRequired")));

        return Task.FromResult(vr);
    }

    public Task<ValidationResult> ValidateForActivateAsync(JwtKeyEntity entity)
    {
        var vr = new ValidationResult();
        if (entity is null)
        {
            vr.Errors.Add(new ValidationFailure("JwtKey", _localization.GetMessage("Domain.JwtKey.EntityRequired")));
            return Task.FromResult(vr);
        }

        if (entity.IsDeleted)
            vr.Errors.Add(new ValidationFailure(nameof(entity.IsDeleted), _localization.GetMessage("Domain.JwtKey.CannotActivateDeleted")));

        return Task.FromResult(vr);
    }

    public Task<ValidationResult> ValidateForDeactivateAsync(JwtKeyEntity entity)
    {
        // Sem regras adicionais por enquanto
        return Task.FromResult(new ValidationResult());
    }

    public Task<ValidationResult> ValidateForDeleteAsync(JwtKeyEntity entity)
    {
        var vr = new ValidationResult();
        if (entity is null)
        {
            vr.Errors.Add(new ValidationFailure("JwtKey", _localization.GetMessage("Domain.JwtKey.EntityRequired")));
            return Task.FromResult(vr);
        }

        if (entity.IsActive)
            vr.Errors.Add(new ValidationFailure(nameof(entity.IsActive), _localization.GetMessage("Domain.JwtKey.CannotDeleteActive")));

        return Task.FromResult(vr);
    }

    public Task<ValidationResult> ValidateForRevokeAsync(JwtKeyEntity entity)
    {
        var vr = new ValidationResult();
        if (entity is null)
        {
            vr.Errors.Add(new ValidationFailure("JwtKey", _localization.GetMessage("Domain.JwtKey.EntityRequired")));
            return Task.FromResult(vr);
        }

        if (entity.IsRevoked())
            vr.Errors.Add(new ValidationFailure("IsRevoked", _localization.GetMessage("Domain.JwtKey.AlreadyRevoked")));

        return Task.FromResult(vr);
    }
}
