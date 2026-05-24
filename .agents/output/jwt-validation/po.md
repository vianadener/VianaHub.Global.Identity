# PO Report - JWT Validation

## Feature / Documentation Exists
- Existe documentação e implementação para JWT no repositório.
- Arquivos principais encontrados:
  - `src/VianaHub.Global.Identity.Api/Configuration/JwtSetup.cs`
  - `src/VianaHub.Global.Identity.Application/Services/JwtTokenService.cs`
  - `src/VianaHub.Global.Identity.Application/Services/RefreshTokenService.cs`
  - `src/VianaHub.Global.Identity.Application/Services/JwtKeyAppService.cs`
  - `src/VianaHub.Global.Identity.Domain/Entities/JwtKeyEntity.cs`
  - `src/VianaHub.Global.Identity.Domain/Entities/RefreshTokenEntity.cs`
  - `tests/VianaHub.Global.Identity.Tests/Application/JwtTokenServiceTests.cs`
  - `tests/VianaHub.Global.Identity.Tests/Application/RefreshTokenServiceTests.cs`
  - `tests/VianaHub.Global.Identity.Tests/Application/JwtKeyAppServiceTests.cs`

## Scope Observed
- JWT access token generation and validation.
- Refresh token issue, rotate and revoke.
- JWT key storage, rotation and validation.
- Authorization based on roles/permissions embedded in the token.

## Notes
- The repository is already structured to support the JWT lifecycle.
- The validation task is therefore a review of an already implemented feature, not a blank-slate feature creation.

