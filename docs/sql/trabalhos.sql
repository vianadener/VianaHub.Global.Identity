EXEC sp_set_session_context @key=N'IsSuperAdmin', @value=1;
select * from dbo.Tenants;
select * from dbo.Apps;
select * from dbo.JobDefinitions;
select * from dbo.Roles;			-- 7
select * from dbo.Resources;		-- 27
select * from dbo.Actions;			-- 18
select * from dbo.Users;
select * from dbo.UserRoles;
select * from dbo.RolePermissions;
select * from dbo.JwtKeys;
select * from dbo.RefreshTokens order by AddedOn desc;

select distinct l1.id "UserId", 
				l1.Name "User",
				l3.Id "AppId",
				l3.Name "App",
				l2.RoleId "RoleId",
				l4.Name "Role",
				l6.Id "ResourceId",
				l6.Name "Resource",
				l7.Id "ActionId",
				l7.Name "Action"
from dbo.Users				l1
join dbo.UserRoles			l2 on l2.UserId = l1.Id
join dbo.Apps				l3 on l3.Id = l2.AppId
join dbo.Roles				l4 on l4.Id = l2.RoleId
join dbo.RolePermissions	l5 on l5.RoleId = l4.Id
join dbo.Resources			l6 on l6.Id = l5.ResourceId
join dbo.Actions			l7 on l7.Id = l5.ActionId
where 1=1
  and l1.id = 3 
;