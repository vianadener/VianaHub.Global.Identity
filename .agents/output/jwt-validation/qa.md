# QA Report - JWT Validation

## Test Scenarios
- Generate access token for a valid user and tenant.
- Validate that the token contains `sub`, `tenantId`, `appId`, `name` and `jti`.
- Validate that roles and permissions are included when the user has assignments.
- Validate that access token generation fails when there is no active JWT key.
- Validate that refresh tokens are issued hashed and persisted.
- Validate refresh token rotation revokes the old token and creates a new one.
- Validate refresh token rotation rejects invalid or expired tokens.
- Validate revoke-all by user and tenant.
- Validate JWT key validation, activation and rotation behavior.
- Validate API auth setup rejects missing/invalid tokens and honors configured issuers/audiences.

## Edge Cases
- Missing master key for JWT decryption.
- Corrupted private key data.
- No permissions available for the user.
- Tenant mismatch during key lookup.
- Expired or revoked JWT key used for validation.

## Coverage Expectation
- Unit tests: good coverage already exists for token, refresh token and key services.
- Integration tests: should verify end-to-end authentication and authorization paths.
- Security checks: should cover tenant isolation and privilege boundaries.

## QA Verdict
- The test surface is adequate for the feature.
- The main missing risk area is explicit regression coverage for privilege-escalation attempts via manipulated claims.

