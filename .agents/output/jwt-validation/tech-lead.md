# Tech Lead Report - JWT Validation

## Architecture Review
- The JWT flow follows Clean Architecture boundaries reasonably well:
  - API config in `JwtSetup`
  - token generation in `JwtTokenService`
  - refresh token lifecycle in `RefreshTokenService`
  - key management in `JwtKeyAppService` / domain entities / repositories
- Domain entities carry the security-sensitive state:
  - `JwtKeyEntity`
  - `RefreshTokenEntity`
- The flow is consistent with DDD usage, but there is a meaningful coupling between authentication middleware and tenant-context resolution.

## Key Technical Observations
- `JwtSetup` resolves tenant information from the raw token before full validation to query signing keys.
- This is technically justified because the repository uses tenant isolation.
- `JwtTokenService` builds claims including tenant and app context, plus role/permission payload.
- `RefreshTokenService` stores only hashed tokens, which is the right pattern.

## Risks
- The pre-validation tenant extraction in `JwtSetup` is security-sensitive and must remain tightly controlled.
- The `permissions` payload is embedded into the JWT; this increases token size and makes claim governance important.
- The code relies heavily on correct configuration of `JwtSettings`, `ISecretProvider`, and tenant-aware repositories.

## Conclusion
- The JWT design is aligned with the repository architecture.
- No architecture rewrite is required for validation.
- The main concerns are correctness of tenant isolation, rotation and permission claims, not structural violations.

