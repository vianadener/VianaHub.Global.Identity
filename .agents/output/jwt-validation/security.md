# Security Report - JWT Validation

## Findings
- JWT access tokens are signed with RSA and tenant-specific keys.
- Refresh tokens are hashed before persistence.
- Authorization is permission/role based and embedded into the token payload.
- `JwtSetup` extracts tenant identity from the raw token before full validation to load signing keys.

## Vulnerability Review
- No direct indication of secret hardcoding in the reviewed JWT flow.
- No obvious SQL injection or XSS vector in the JWT code path.
- Tenant isolation depends on the correctness of the early tenant extraction logic.
- Privilege escalation risk exists if claims are trusted without adequate server-side enforcement in authorization filters and repositories.
- The permissions payload inside the token can become a governance risk if authorization checks rely solely on token contents instead of server-side evaluation.

## OWASP-Oriented Notes
- Password/token storage: refresh token hashing is good.
- Authentication: JWT validation is configured with issuer, audience and signature checks.
- Access control: acceptable but must remain server-enforced.
- Sensitive data: private keys are encrypted at rest.
- Key management: key rotation jobs exist, which is positive.

## Recommendations
- Keep authorization server-side and do not rely only on JWT claims for sensitive decisions.
- Add explicit regression tests for privilege-escalation attempts and tenant-mismatch scenarios.
- Monitor token size growth due to embedded permissions payload.

## Status
- approved with remarks

