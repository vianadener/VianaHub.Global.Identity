/* =========================================================
   1. DESATIVAR E REMOVER ROW LEVEL SECURITY
   ========================================================= */

IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantSecurityPolicy')
BEGIN
    DROP SECURITY POLICY dbo.TenantSecurityPolicy;
END
GO
DROP SECURITY POLICY IF EXISTS dbo.TenantSecurityPolicy;
GO
DROP FUNCTION IF EXISTS dbo.fn_TenantAccessPredicate;
GO
DROP TABLE IF EXISTS dbo.JobDefinitions;
DROP TABLE IF EXISTS dbo.JwtKeys;
DROP TABLE IF EXISTS dbo.RefreshTokens;
DROP TABLE IF EXISTS dbo.UserRoles;
DROP TABLE IF EXISTS dbo.RolePermissions;
DROP TABLE IF EXISTS dbo.Actions;
DROP TABLE IF EXISTS dbo.Resources;
DROP TABLE IF EXISTS dbo.Roles;
DROP TABLE IF EXISTS dbo.Users;
DROP TABLE IF EXISTS dbo.Apps;
DROP TABLE IF EXISTS dbo.Tenants;



GO
