-- A aplicação Identity é um sistema de autenticação e autorização que permite gerenciar usuários, papéis, recursos e permissões de forma segura e eficiente. O banco de dados é projetado para suportar multi-tenancy, permitindo que múltiplos tenants (clientes) utilizem o sistema de forma isolada. Cada tabela criada tem um propósito específico dentro do sistema:
-- A aplicação Identity é um cliente do próprio sistema de autenticação e autorização, utilizando os recursos, ações, papéis e permissões para gerenciar o acesso dos usuários à própria aplicação. Dessa forma, a aplicação Identity pode ser configurada como um tenant dentro do sistema, permitindo que ela utilize as funcionalidades de autenticação e autorização de forma segmentada e segura, garantindo que os usuários da aplicação Identity tenham acesso apenas aos recursos e ações permitidos para o tenant da aplicação Identity.


-- A tabela Tenants é responsável por armazenar as informações dos tenants (clientes) que utilizam o sistema de autenticação e autorização. Cada tenant representa uma organização ou cliente distinto que utiliza o sistema, permitindo a implementação de multi-tenancy. A tabela Tenants possui uma relação de um para muitos com as tabelas Apps, Users, Roles, Resources, Actions, RolePermissions, UserRoles, RefreshTokens e JwtKeys, garantindo que cada tenant possa ter suas próprias aplicações, usuários, papéis, recursos, ações, permissões de papéis, papéis de usuários, tokens de atualização e chaves JWT de forma isolada e segura.
CREATE TABLE dbo.Tenants (											            -- Tabela de Tenants para multi-tenancy
    Id			        INT IDENTITY(1,1)	NOT NULL,				            -- Chave primária auto-incrementada
    Name	            NVARCHAR(200)		NOT NULL,				            -- Nome do tenant, único
    Description         NVARCHAR(500)		NOT NULL,                           -- Descrição do tenant
	Alias               NVARCHAR(30)        NOT NULL,                           -- Alias curto para o tenant, único
    UrlImage    	    NVARCHAR(500)			NULL,                           -- URL da imagem do tenant, opcional
    Settings            NVARCHAR(MAX)			NULL,                           -- Configurações adicionais em formato JSON, opcional
    Remarks		        NVARCHAR(1000)		    NULL,				            -- Observações adicionais, opcional
    IsActive	        BIT					NOT NULL DEFAULT 1,                 -- Indica se o tenant está ativo, padrão é 1 (ativo)
    IsDeleted	        BIT					NOT NULL DEFAULT 0,                 -- Indica se o tenant está deletado, padrão é 0 (não deletado)
    AddedBy	            INT   		        NOT NULL,						    -- ID do usuário que adicionou o tenant
    AddedOn	            DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),	    -- Data e hora em que o tenant foi adicionado, padrão é a data e hora atual
    ModifiedBy	        INT       		        NULL,							-- ID do usuário que modificou o tenant pela última vez, opcional
    ModifiedAt	        DATETIME2(7)		    NULL,							-- Data e hora da última modificação do tenant, opcional
	CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id),                           -- Chave primária na coluna Id
    CONSTRAINT CK_Tenants_Settings_IsJson CHECK (Settings IS NULL OR ISJSON(Settings) = 1), -- Verifica se o campo Settings é nulo ou contém um JSON válido
    CONSTRAINT CK_Tenants_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1))       -- Garante que um tenant não possa ser ativo e deletado ao mesmo tempo
);
GO
-- A tabela Apps é responsável por armazenar as informações das aplicações de cada tenant. Cada aplicação representa um sistema ou serviço específico dentro do tenant, permitindo a organização e gerenciamento de recursos, ações, papéis e permissões de forma segmentada por aplicação. A tabela Apps possui uma relação de um para muitos com as tabelas Roles, Resources, Actions e RolePermissions, garantindo que cada aplicação possa ter seus próprios papéis, recursos, ações e permissões de papéis de forma isolada e segura dentro do tenant.
CREATE TABLE dbo.Apps (											                -- Tabela de Aplicações para cada tenant
    Id			        INT IDENTITY(1,1)	NOT NULL,			                -- Chave primária auto-incrementada
    TenantId		    INT					NOT NULL,                           -- Chave estrangeira para o tenant ao qual a aplicação pertence
    Name				NVARCHAR(200)		NOT NULL,			                -- Nome da aplicação, único dentro do tenant
    Description         NVARCHAR(500)		NOT NULL,                           -- Descrição da aplicação
    IsActive	        BIT					NOT NULL DEFAULT 1,                 -- Indica se a aplicação está ativa, padrão é 1 (ativa)
    IsDeleted	        BIT					NOT NULL DEFAULT 0,                 -- Indica se a aplicação está deletada, padrão é 0 (não deletada)
    AddedBy	            INT   		        NOT NULL,						    -- ID do usuário que adicionou a aplicação
    AddedOn	            DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),	    -- Data e hora em que a aplicação foi adicionada, padrão é a data e hora atual
    ModifiedBy	        NVARCHAR(50)       		NULL,						    -- ID do usuário que modificou a aplicação pela última vez, opcional
    ModifiedAt	        DATETIME2(7)		    NULL,						    -- Data e hora da última modificação da aplicação, opcional
	CONSTRAINT PK_Apps PRIMARY KEY CLUSTERED (Id),                              -- Chave primária na coluna Id
    CONSTRAINT UQ_Apps_Tenant_Id UNIQUE (TenantId, Id),			                -- Garante que o Id seja único dentro do tenant
    CONSTRAINT UQ_Apps_Tenant_Name UNIQUE (TenantId, Name),                     -- Garante que o nome da aplicação seja único dentro do tenant
    CONSTRAINT CK_Apps_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)), -- Garante que uma aplicação não possa ser ativa e deletada ao mesmo tempo
    CONSTRAINT FK_Apps_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)     -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
);
GO
-- A tabela Users é responsável por armazenar as informações dos usuários de cada tenant. Cada usuário representa uma pessoa ou entidade que tem acesso ao sistema de autenticação e autorização, permitindo a gestão de identidades, credenciais e perfis de acesso de forma segmentada por tenant. A tabela Users possui uma relação de um para muitos com as tabelas UserRoles e RefreshTokens, garantindo que cada usuário possa ter seus próprios papéis e tokens de atualização de forma isolada e segura dentro do tenant.
CREATE TABLE dbo.Users (                                                            -- Tabela de Usuários para cada tenant
    Id                          INT IDENTITY(1,1)   NOT NULL,                       -- Chave primária auto-incrementada
    TenantId                    INT                 NOT NULL,                       -- Chave estrangeira para o tenant ao qual o usuário pertence
    Name                        NVARCHAR(256)       NOT NULL,                       -- Nome completo do usuário
    LoginIdentifier             NVARCHAR(500)       NOT NULL,                       -- Identificador de login do usuário (pode ser email ou username)
    NormalizedLoginIdentifier   NVARCHAR(500)       NOT NULL,                       -- Versão normalizada do identificador de login para garantir unicidade sem considerar maiúsculas/minúsculas
    PasswordHash                NVARCHAR(500)       NOT NULL,       	            -- Hash da senha do usuário
    Email                       NVARCHAR(500)       NULL,                           -- Email do usuário, opcional
    UrlImage                    NVARCHAR(500)           NULL,                       -- URL da imagem do usuário, opcional
    LastAccessAt                DATETIME2(7)            NULL,                       -- Data e hora do último acesso do usuário, opcional
    AccessFailedCount           INT                 NOT NULL DEFAULT 0,             -- Contador de falhas de acesso, padrão é 0
    IsActive                    BIT                 NOT NULL DEFAULT 1,             -- Indica se o usuário está ativo, padrão é 1 (ativo)
    IsDeleted                   BIT                 NOT NULL DEFAULT 0,             -- Indica se o usuário está deletado, padrão é 0 (não deletado)
    AddedBy	                    INT                 NOT NULL,                       -- ID do usuário que adicionou este usuário
    AddedOn                     DATETIME2(7)        NOT NULL DEFAULT SYSDATETIME(), -- Data e hora em que o usuário foi adicionado, padrão é a data e hora atual
    ModifiedBy                  INT                     NULL,                       -- ID do usuário que modificou este usuário pela última vez, opcional
    ModifiedAt                  DATETIME2(7)            NULL,                       -- Data e hora da última modificação deste usuário, opcional
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),                                 -- Chave primária na coluna Id
    CONSTRAINT CK_Users_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),-- Garante que um usuário não possa ser ativo e deletado ao mesmo tempo
    CONSTRAINT UQ_Users_Id_Tenant UNIQUE (Id, TenantId),                            -- Garante que o Id seja único dentro do tenant
    CONSTRAINT UQ_Users_Tenant_NormalizedLoginIdentifier UNIQUE (TenantId, NormalizedLoginIdentifier),  -- Garante que o identificador de login normalizado seja único dentro do tenant
    CONSTRAINT FK_Users_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)                        -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
);
GO
-- A tabela Roles é responsável por armazenar as informações dos papéis de cada usuário de aplicação do tenant. Cada papel representa um conjunto de permissões que podem ser atribuídas a usuários para controlar o acesso a recursos e ações dentro da aplicação. A tabela Roles possui uma relação de um para muitos com as tabelas RolePermissions e UserRoles, garantindo que cada papel possa ter suas próprias permissões e ser atribuído a múltiplos usuários de forma isolada e segura dentro do tenant e aplicação.
CREATE TABLE dbo.Roles (												            -- Tabela de Papéis para cada tenant e aplicação
    Id				    INT IDENTITY(1,1)	NOT NULL,						        -- Chave primária auto-incrementada
    TenantId		    INT					NOT NULL,						        -- Chave estrangeira para o tenant ao qual o papel pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual o papel pertence
    Name			    NVARCHAR(100)		NOT NULL,						        -- Nome do papel, único dentro da aplicação e tenant
    Description         NVARCHAR(500)		NOT NULL,						        -- Descrição do papel
    IsActive		    BIT					NOT NULL DEFAULT 1,                     -- Indica se o papel está ativo, padrão é 1 (ativo)
    IsDeleted		    BIT					NOT NULL DEFAULT 0,                     -- Indica se o papel está deletado, padrão é 0 (não deletado)
    AddedBy		        INT                 NOT NULL,						        -- ID do usuário que adicionou o papel
    AddedOn		        DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),	        -- Data e hora em que o papel foi adicionado, padrão é a data e hora atual
    ModifiedBy	        INT   		            NULL,						        -- ID do usuário que modificou o papel pela última vez, opcional
    ModifiedAt		    DATETIME2(7)			NULL,                               -- Data e hora da última modificação do papel, opcional
	CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id),		                            -- Chave primária na coluna Id
    CONSTRAINT CK_Roles_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),-- Garante que um papel não possa ser ativo e deletado ao mesmo tempo
    CONSTRAINT UQ_Roles_Id_Tenant UNIQUE (Id, TenantId),                            -- Garante que o Id seja único dentro do tenant
    CONSTRAINT UQ_Roles_Tenant_AppId_Name UNIQUE (TenantId, AppId, Name),		    -- Garante que o nome do papel seja único dentro da aplicação e tenant
    CONSTRAINT UQ_Roles_Tenant_AppId_Id UNIQUE (TenantId, AppId, Id),			    -- Garante que o Id seja único dentro da aplicação e tenant
    CONSTRAINT FK_Roles_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),	-- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
    CONSTRAINT FK_Roles_Apps FOREIGN KEY (TenantId, AppId) REFERENCES dbo.Apps(TenantId, Id)    -- Chave estrangeira para garantir integridade referencial com a tabela de Apps
);
GO
-- A tabela Resources é responsável por armazenar as informações dos recursos de cada aplicação do tenant. Cada recurso representa um elemento ou funcionalidade específica dentro da aplicação, permitindo a gestão de permissões de acesso de forma segmentada por recurso. A tabela Resources possui uma relação de um para muitos com a tabela RolePermissions, garantindo que cada recurso possa ter suas próprias permissões atribuídas a diferentes papéis de forma isolada e segura dentro do tenant e aplicação.
CREATE TABLE dbo.Resources (											            -- Tabela de Recursos para cada tenant e aplicação
    Id				    INT IDENTITY(1,1)	NOT NULL,						        -- Chave primária auto-incrementada
    TenantId		    INT					NOT NULL,						        -- Chave estrangeira para o tenant ao qual o recurso pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual o recurso pertence
    Name			    NVARCHAR(200)		NOT NULL,				                -- Nome do recurso, único dentro da aplicação e tenant
    Description         NVARCHAR(500)		NOT NULL,						        -- Descrição do recurso
    IsActive		    BIT					NOT NULL DEFAULT 1,                     -- Indica se o recurso está ativo, padrão é 1 (ativo)
    IsDeleted		    BIT					NOT NULL DEFAULT 0,                     -- Indica se o recurso está deletado, padrão é 0 (não deletado)
    AddedBy		        INT                 NOT NULL,						        -- ID do usuário que adicionou o recurso
    AddedOn		        DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),	        -- Data e hora em que o recurso foi adicionado, padrão é a data e hora atual
    ModifiedBy	        INT  		            NULL,						        -- ID do usuário que modificou o recurso pela última vez, opcional
    ModifiedAt		    DATETIME2(7)			NULL,						        -- Data e hora da última modificação do recurso, opcional
	CONSTRAINT PK_Resources PRIMARY KEY CLUSTERED (Id),					            -- Chave primária na coluna Id
    CONSTRAINT CK_Resources_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),            -- Garante que um recurso não possa ser ativo e deletado ao mesmo tempo
	CONSTRAINT UQ_Resources_Tenant_AppId_Name UNIQUE (TenantId, AppId, Name),                       -- Garante que o nome do recurso seja único dentro da aplicação e tenant
    CONSTRAINT UQ_Resources_Tenant_AppId_Id UNIQUE (TenantId, AppId, Id),                           -- Garante que o Id seja único dentro da aplicação e tenant
    CONSTRAINT FK_Resources_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),               -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
    CONSTRAINT FK_Resources_Apps FOREIGN KEY (TenantId, AppId) REFERENCES dbo.Apps(TenantId, Id)    -- Chave estrangeira para garantir integridade referencial com a tabela de Apps
);
GO
-- A tabela Actions é responsável por armazenar as informações das ações de cada aplicação do tenant. Cada ação representa uma operação ou funcionalidade específica que pode ser realizada dentro da aplicação, permitindo a gestão de permissões de acesso de forma segmentada por ação. A tabela Actions possui uma relação de um para muitos com a tabela RolePermissions, garantindo que cada ação possa ter suas próprias permissões atribuídas a diferentes papéis de forma isolada e segura dentro do tenant e aplicação.
CREATE TABLE dbo.Actions (												            -- Tabela de Ações para cada tenant e aplicação
    Id				    INT IDENTITY(1,1)	NOT NULL,						        -- Chave primária auto-incrementada
    TenantId		    INT					NOT NULL,						        -- Chave estrangeira para o tenant ao qual a ação pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual a ação pertence
    Name			    NVARCHAR(50)		NOT NULL,                               -- Nome da ação, único dentro da aplicação e tenant
    Description         NVARCHAR(500)		NOT NULL,                               -- Descrição da ação
    IsActive		    BIT					NOT NULL DEFAULT 1,                     -- Indica se a ação está ativa, padrão é 1 (ativo)
    IsDeleted		    BIT					NOT NULL DEFAULT 0,                     -- Indica se a ação está deletada, padrão é 0 (não deletado)
    AddedBy		        INT                 NOT NULL,						        -- ID do usuário que adicionou a ação
    AddedOn		        DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),	        -- Data e hora em que a ação foi adicionada, padrão é a data e hora atual
    ModifiedBy	        INT   		            NULL,						        -- ID do usuário que modificou a ação pela última vez, opcional
    ModifiedAt		    DATETIME2(7)			NULL,                               -- Data e hora da última modificação da ação, opcional
	CONSTRAINT PK_Actions PRIMARY KEY CLUSTERED (Id),					            -- Chave primária na coluna Id
    CONSTRAINT CK_Actions_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),          -- Garante que uma ação não possa ser ativa e deletada ao mesmo tempo
	CONSTRAINT UQ_Actions_Tenant_AppId_Name UNIQUE (TenantId, AppId, Name),                     -- Garante que o nome da ação seja único dentro da aplicação e tenant
    CONSTRAINT UQ_Actions_Tenant_AppsId_Id UNIQUE (TenantId, AppId, Id),	                    -- Garante que o Id seja único dentro da aplicação e tenant
    CONSTRAINT FK_Actions_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),	            -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
    CONSTRAINT FK_Actions_Apps FOREIGN KEY (TenantId, AppId) REFERENCES dbo.Apps(TenantId, Id)  -- Chave estrangeira para garantir integridade referencial com a tabela de Apps
);
GO
-- A tabela RolePermissions é responsável por armazenar as permissões de cada papel dentro de uma aplicação do tenant. Cada permissão relaciona um papel, um recurso e uma ação, permitindo a gestão de acesso de forma granular. A tabela RolePermissions possui relações de muitos para um com as tabelas Roles, Resources e Actions, garantindo que cada permissão esteja associada a um papel, recurso e ação específicos dentro do tenant e aplicação.
CREATE TABLE dbo.RolePermissions (                                                  -- Tabela de Permissões de Papéis para cada tenant e aplicação, relacionando papéis, recursos e ações
    Id                  INT IDENTITY(1,1)   NOT NULL,                               -- Chave primária auto-incrementada
    TenantId            INT                 NOT NULL,                               -- Chave estrangeira para o tenant ao qual a permissão pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual a permissão pertence
    RoleId              INT                 NOT NULL,                               -- Chave estrangeira para o papel ao qual a permissão pertence
    ResourceId          INT                 NOT NULL,                               -- Chave estrangeira para o recurso ao qual a permissão pertence
    ActionId            INT                 NOT NULL,                               -- Chave estrangeira para a ação ao qual a permissão pertence
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (Id),                       -- Chave primária na coluna Id
    CONSTRAINT UQ_RolePermissions UNIQUE (TenantId, AppId, RoleId, ResourceId, ActionId),                                               -- Garante que a combinação de RoleId, ResourceId e ActionId seja única dentro da aplicação e tenant
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (TenantId, AppId, RoleId) REFERENCES dbo.Roles (TenantId, AppId, Id),                -- Chave estrangeira para garantir integridade referencial com a tabela de Roles
    CONSTRAINT FK_RolePermissions_Resource FOREIGN KEY (TenantId, AppId, ResourceId) REFERENCES dbo.Resources ( TenantId, AppId, Id),   -- Chave estrangeira para garantir integridade referencial com a tabela de Resources
    CONSTRAINT FK_RolePermissions_Action FOREIGN KEY (TenantId, AppId, ActionId) REFERENCES dbo.Actions (TenantId, AppId, Id)           -- Chave estrangeira para garantir integridade referencial com a tabela de Actions
);
GO
-- A tabela UserRoles é responsável por armazenar os papéis atribuídos a cada usuário dentro de uma aplicação do tenant. Cada registro relaciona um usuário a um papel específico, permitindo a gestão de acesso de forma granular. A tabela UserRoles possui relações de muitos para um com as tabelas Users e Roles, garantindo que cada usuário possa ter múltiplos papéis e cada papel possa ser atribuído a múltiplos usuários dentro do tenant e aplicação.
CREATE TABLE dbo.UserRoles (                                                        -- Tabela de Papéis de Usuários para cada tenant e aplicação, relacionando usuários e papéis
    Id                  INT IDENTITY(1,1)   NOT NULL,                               -- Chave primária auto-incrementada
    TenantId            INT                 NOT NULL,                               -- Chave estrangeira para o tenant ao qual o papel pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual o papel pertence
    UserId              INT                 NOT NULL,                               -- Chave estrangeira para o usuário ao qual o papel pertence
    RoleId              INT                 NOT NULL,                               -- Chave estrangeira para o papel ao qual o usuário pertence
    CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (Id),                             -- Chave primária na coluna Id
    CONSTRAINT UQ_UserRoles UNIQUE (TenantId, AppId, UserId, RoleId),               -- Garante que a combinação de UserId e RoleId seja única dentro da aplicação e tenant
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId, TenantId) REFERENCES dbo.Users (Id, TenantId),                -- Chave estrangeira para garantir integridade referencial com a tabela de Users
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (TenantId, AppId, RoleId) REFERENCES dbo.Roles (TenantId, AppId, Id),  -- Chave estrangeira para garantir integridade referencial com a tabela de Roles
    CONSTRAINT FK_UserRoles_Apps FOREIGN KEY (TenantId, AppId) REFERENCES dbo.Apps (TenantId, Id)                   -- Chave estrangeira para garantir integridade referencial com a tabela de Apps
);
GO
-- A tabela RefreshTokens é responsável por armazenar os tokens de atualização para cada usuário dentro de uma aplicação do tenant. Cada token de atualização permite que um usuário obtenha um novo token de acesso sem precisar fazer login novamente. A tabela RefreshTokens possui relações de muitos para um com as tabelas Users e Apps, garantindo que cada token esteja associado a um usuário e aplicação específicos dentro do tenant.
CREATE TABLE dbo.RefreshTokens (                                                    -- Tabela de Tokens de Atualização para cada tenant, aplicação e usuário
    Id 					INT IDENTITY(1,1)	NOT NULL,                               -- Chave primária auto-incrementada
    TenantId 			INT					NOT NULL,                               -- Chave estrangeira para o tenant ao qual o token pertence
    AppId				INT                 NOT NULL,                               -- Chave estrangeira para a aplicação à qual o token pertence
	UserId 				INT					NOT NULL,                               -- Chave estrangeira para o usuário ao qual o token pertence
	TokenHash           VARBINARY(64)       NOT NULL,                               -- Hash do token de atualização
	ExpiresAt			DATETIME2(7) 		NOT	NULL,                               -- Data e hora de expiração do token
    RevokedAt 			DATETIME2(7) 			NULL,                               -- Data e hora de revogação do token
	RevokedBy			INT			            NULL,                               -- ID do usuário que revogou o token, opcional
    AddedBy		        INT                 NOT NULL,                               -- ID do usuário que adicionou o token
    AddedOn		        DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),         -- Data e hora em que o token foi adicionado, padrão é a data e hora atual
    ModifiedBy	        INT   		            NULL,                               -- ID do usuário que modificou o token pela última vez, opcional
    ModifiedAt			DATETIME2(7)			NULL,                               -- Data e hora da última modificação do token, opcional
    CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),                         -- Chave primária na coluna Id
    CONSTRAINT UQ_RefreshTokens UNIQUE (TenantId, AppId, TokenHash),                -- Garante que o hash do token seja único dentro da aplicação e tenant
    CONSTRAINT FK_RefreshTokens_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),                       -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
	CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId, TenantId) REFERENCES dbo.Users(Id, TenantId),         -- Chave estrangeira para garantir integridade referencial com a tabela de Users
    CONSTRAINT FK_RefreshTokens_RevokedBy FOREIGN KEY (RevokedBy, TenantId) REFERENCES dbo.Users(Id, TenantId), -- Chave estrangeira para garantir integridade referencial com a tabela de Users para o usuário que revogou o token
    CONSTRAINT FK_RefreshTokens_Apps FOREIGN KEY (TenantId, AppId) REFERENCES dbo.Apps(TenantId, Id)            -- Chave estrangeira para garantir integridade referencial com a tabela de Apps
);
GO
-- A tabela PasswordResetTokens armazena os tokens de recuperação de senha (one-time use, TTL de 15 minutos). O token bruto é gerado como UUID, hash SHA-256 é persistido para evitar comprometimento em caso de vazamento do banco. Cada token está vinculado a um tenant e usuário específicos. Após uso ou expiração, o token é marcado como utilizado (Used = 1) e não pode mais ser reutilizado.
CREATE TABLE dbo.PasswordResetTokens (                                              -- Tabela de Tokens de Recuperação de Senha — one-time use, TTL de 15 minutos
    Id          INT IDENTITY(1,1)   NOT NULL,                                       -- Chave primária auto-incrementada
    TenantId    INT                 NOT NULL,                                       -- Chave estrangeira para o tenant ao qual o token pertence
    UserId      INT                 NOT NULL,                                       -- Chave estrangeira para o usuário ao qual o token pertence
    TokenHash   VARBINARY(64)       NOT NULL,                                       -- Hash SHA-256 do token bruto (UUID), nunca armazenar o token em texto claro
    ExpiresAt   DATETIME2(7)        NOT NULL,                                       -- Data e hora de expiração do token (TTL de 15 minutos a partir da criação)
    Used        BIT                 NOT NULL DEFAULT 0,                             -- Indica se o token já foi utilizado; após uso torna-se inválido (one-time use)
    AddedBy     INT                 NOT NULL,                                       -- ID do usuário que gerou o token (0 = sistema)
    AddedOn     DATETIME2(7)        NOT NULL DEFAULT SYSDATETIME(),                 -- Data e hora de criação do token
    ModifiedBy  INT                     NULL,                                       -- ID do usuário que marcou o token como usado, opcional
    ModifiedAt  DATETIME2(7)            NULL,                                       -- Data e hora em que o token foi marcado como usado, opcional
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY CLUSTERED (Id),                   -- Chave primária na coluna Id
    CONSTRAINT UQ_PasswordResetTokens_TenantId_TokenHash UNIQUE (TenantId, TokenHash),                              -- Garante que o hash do token seja único dentro do tenant
    CONSTRAINT FK_PasswordResetTokens_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),                     -- Integridade referencial com Tenants
    CONSTRAINT FK_PasswordResetTokens_User  FOREIGN KEY (UserId, TenantId) REFERENCES dbo.Users(Id, TenantId)       -- Integridade referencial com Users dentro do mesmo tenant
);
GO
CREATE TABLE dbo.JwtKeys (                                                          -- Tabela de Chaves JWT para cada tenant, utilizada para assinatura de tokens JWT
    Id 						INT IDENTITY(1,1)	NOT NULL,                           -- Chave primária auto-incrementada
    TenantId 				INT					NOT NULL,                           -- Chave estrangeira para o tenant ao qual a chave pertence
    KeyId 					UNIQUEIDENTIFIER	NOT NULL,                           -- Identificador único da chave, utilizado para referência na assinatura de tokens JWT
    PublicKey 				NVARCHAR(MAX) 		NOT NULL,                           -- Chave pública em formato PEM, utilizada para validação de tokens JWT
    PrivateKeyEncrypted 	NVARCHAR(MAX) 		NOT NULL,                           -- Chave privada criptografada em formato PEM, utilizada para assinatura de tokens JWT
    Algorithm 				NVARCHAR(50) 		NOT NULL DEFAULT 'RS256',           -- Algoritmo de assinatura utilizado, padrão é RS256
    KeySize 				INT 				NOT NULL DEFAULT 2048,              -- Tamanho da chave em bits, padrão é 2048
    KeyType 				NVARCHAR(50) 		NOT NULL DEFAULT 'RSA',             -- Tipo da chave, padrão é RSA
    RevokedReason 			NVARCHAR(500) 			NULL,                           -- Razão da revogação da chave, opcional
    UsageCount 				BIGINT 				NOT NULL DEFAULT 0,                 -- Contador de uso da chave para assinatura de tokens JWT, padrão é 0
    ActivatedAt 			DATETIME2(7) 				NULL,                       -- Data e hora de ativação da chave, opcional
    ExpiresAt 				DATETIME2(7) 			NOT NULL,                       -- Data e hora de expiração da chave, obrigatória
    LastUsedAt 				DATETIME2(7) 				NULL,                       -- Data e hora do último uso da chave para assinatura de tokens JWT, opcional
    NextRotationAt 			DATETIME2(7) 			NOT NULL,                       -- Data e hora prevista para a próxima rotação da chave, obrigatória
    RevokedAt 				DATETIME2(7) 				NULL,                       -- Data e hora de revogação da chave, opcional
    LastValidatedAt 		DATETIME2(7) 				NULL,                       -- Data e hora da última validação da chave para tokens JWT, opcional
    ValidationCount 		BIGINT 				NOT NULL DEFAULT 0,                 -- Contador de validação da chave para tokens JWT, padrão é 0
    RotationPolicyDays 		INT 				NOT NULL DEFAULT 90,                -- Número de dias para a rotação automática da chave, padrão é 90
    OverlapPeriodDays 		INT 				NOT NULL DEFAULT 7,                 -- Número de dias de sobreposição entre a chave antiga e a nova durante a rotação, padrão é 7
    MaxTokenLifetimeMinutes INT 				NOT NULL DEFAULT 60,                -- Tempo máximo de vida dos tokens JWT assinados com esta chave, em minutos, padrão é 60
    IsActive 				BIT 				NOT NULL DEFAULT 1,                 -- Indica se a chave está ativa, padrão é 1 (ativa)
    IsDeleted 				BIT 				NOT NULL DEFAULT 0,                 -- Indica se a chave foi excluída, padrão é 0 (não excluída)
    AddedBy		            INT                 NOT NULL,                           -- ID do usuário que adicionou a chave, obrigatório
    AddedOn		            DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),     -- Data e hora de adição da chave, padrão é a data e hora atual
    ModifiedBy	            INT   		            NULL,                           -- ID do usuário que modificou a chave, opcional
    ModifiedAt		        DATETIME2(7)			NULL,                           -- Data e hora da última modificação da chave, opcional 
    CONSTRAINT PK_JwtKeys PRIMARY KEY CLUSTERED (Id),                               -- Chave primária na coluna Id
    CONSTRAINT CK_JwtKeys_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),          -- Garante que uma chave não possa ser ativa e deletada ao mesmo tempo
    CONSTRAINT UQ_JwtKeys_KeyId UNIQUE (TenantId, KeyId),                                       -- Garante que o KeyId seja único dentro do tenant
    CONSTRAINT FK_JwtKeys_Tenant FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),             -- Chave estrangeira para garantir integridade referencial com a tabela de Tenants
    CONSTRAINT CK_JwtKeys_RotationPolicy CHECK (RotationPolicyDays BETWEEN 30 AND 365),         -- Garante que o período de rotação automática da chave seja entre 30 e 365 dias
    CONSTRAINT CK_JwtKeys_OverlapPeriod CHECK (OverlapPeriodDays BETWEEN 1 AND 30),             -- Garante que o período de sobreposição entre a chave antiga e a nova seja entre 1 e 30 dias
    CONSTRAINT CK_JwtKeys_MaxTokenLifetime CHECK (MaxTokenLifetimeMinutes BETWEEN 5 AND 1440)   -- Garante que o tempo máximo de vida dos tokens JWT seja entre 5 minutos e 24 horas (1440 minutos)
);
GO
-- A tabela JobDefinitions é responsável por armazenar as definições de jobs para cada tenant, utilizada para configurar jobs do Hangfire. Cada job possui informações sobre categoria, tipo, nome, descrição, propósito, expressão CRON, configurações, método, fuso horário, execução única, tempo limite, prioridade, fila, número máximo de tentativas, se é um job do sistema, data e hora da última vez que o job foi registrado, status de ativo e deletado, e informações sobre o usuário que adicionou ou modificou o job. A tabela JobDefinitions possui relações de muitos para um com as tabelas Tenants e Users, garantindo que cada job esteja associado a um tenant e usuário específicos.
CREATE TABLE dbo.JobDefinitions (                                                       -- Tabela de Definições de Jobs para cada tenant, utilizada para configurar jobs do Hangfire
    Id                      INT IDENTITY(1,1)	NOT NULL,                               -- Chave primária auto-incrementada
    HangfireJobId           NVARCHAR(100)		    NULL,                               -- Identificador do job no Hangfire, opcional
    Category                NVARCHAR(100)		NOT NULL,                               -- Categoria do job, utilizada para organização e filtragem dos jobs
    Type                    NVARCHAR(200)		NOT NULL,                               -- Tipo do job, utilizado para instanciar o job no Hangfire, deve ser o nome completo da classe do job (incluindo namespace)
    Name                    NVARCHAR(200)		NOT NULL,                               -- Nome do job, único dentro da categoria e tenant, utilizado para identificação do job
    Description             NVARCHAR(1000)		    NULL,                               -- Descrição do job, opcional, utilizada para fornecer informações adicionais sobre o job
    Purpose                 NVARCHAR(1000)		    NULL,                               -- Propósito do job, opcional, utilizada para fornecer informações sobre a finalidade do job
    CronExpression          NVARCHAR(200)		NOT NULL,                               -- Expressão CRON para o agendamento do job, deve ser uma expressão CRON válida, utilizada para configurar a frequência de execução do job no Hangfire
    Configuration           NVARCHAR(MAX)		    NULL,                               -- Configurações adicionais do job em formato JSON, opcional, utilizada para fornecer parâmetros de configuração específicos para o job
    Method                  NVARCHAR(200)		NOT NULL DEFAULT 'Execute',             -- Método a ser executado no job, padrão é 'Execute', deve ser o nome do método público do job que será chamado pelo Hangfire
    TimeZoneId              NVARCHAR(200)		NOT NULL DEFAULT 'GMT Standard Time',   -- TimeZoneId para o agendamento do job, padrão é 'GMT Standard Time', deve ser um TimeZoneId válido do Windows
    ExecuteOnlyOnce         BIT					NOT NULL DEFAULT 0,                     -- Indica se o job deve ser executado apenas uma vez, padrão é 0 (falso)
    TimeoutMinutes          INT					NOT NULL DEFAULT 5,                     -- Tempo limite de execução do job em minutos, padrão é 5, utilizado para configurar o tempo máximo de execução do job antes de ser considerado como falha
    Priority                INT					NOT NULL DEFAULT 5,                     -- Prioridade do job, de 1 (mais alta) a 10 (mais baixa), padrão é 5, utilizada para determinar a ordem de execução dos jobs quando houver múltiplos jobs agendados para o mesmo horário
    Queue                   NVARCHAR(200)		NOT NULL DEFAULT 'default',             -- Fila do Hangfire na qual o job será enfileirado, padrão é 'default', utilizada para organizar os jobs em diferentes filas de execução no Hangfire
    MaxRetries              INT					NOT NULL DEFAULT 3,                     -- Número máximo de tentativas de execução do job em caso de falha, padrão é 3, utilizado para configurar a resiliência do job em caso de falhas temporárias
    IsSystemJob             BIT					NOT NULL DEFAULT 0,                     -- Indica se o job é um job do sistema, que não pode ser modificado ou excluído pelos usuários, padrão é 0 (falso)
    LastRegisteredAt        DATETIME2(7)		    NULL,                               -- Data e hora da última vez que o job foi registrado no Hangfire, opcional, utilizada para monitoramento e auditoria dos jobs
    IsActive                BIT					NOT NULL DEFAULT 1,                     -- Indica se o job está ativo, padrão é 1 (ativo)
    IsDeleted               BIT					NOT NULL DEFAULT 0,                     -- Indica se o job está deletado, padrão é 0 (não deletado)
    AddedBy                 INT                 NOT NULL,                               -- ID do usuário que adicionou o job, obrigatório
    AddedOn                 DATETIME2(7)		NOT NULL DEFAULT SYSDATETIME(),         -- Data e hora em que o job foi adicionado, padrão é a data e hora atual
    ModifiedBy              INT    	                NULL,                               -- ID do usuário que modificou o job pela última vez, opcional
    ModifiedAt              DATETIME2(7)		    NULL,                               -- Data e hora da última modificação do job, opcional
    CONSTRAINT PK_Job PRIMARY KEY CLUSTERED (Id),                                       -- Chave primária na coluna Id
    CONSTRAINT UQ_Job_Name UNIQUE (Name),                                               -- Garante que o nome do job seja único dentro do tenant
    CONSTRAINT CK_Job_Configuration_IsJson CHECK (Configuration IS NULL OR ISJSON(Configuration) = 1),  -- Verifica se o campo Configuration é nulo ou contém um JSON válido
    CONSTRAINT CK_Job_Active_Deleted CHECK (NOT (IsActive = 1 AND IsDeleted = 1)),                      -- Garante que um job não possa ser ativo e deletado ao mesmo tempo
    CONSTRAINT CK_Job_Priority CHECK (Priority BETWEEN 1 AND 10),                                       -- Garante que a prioridade do job seja entre 1 (mais alta) e 10 (mais baixa)
    CONSTRAINT CK_Job_TimeoutMinutes CHECK (TimeoutMinutes > 0),                                        -- Garante que o tempo limite de execução do job seja maior que 0
    CONSTRAINT CK_Job_MaxRetries CHECK (MaxRetries >= 0)                                                -- Garante que o número máximo de tentativas de execução do job seja maior ou igual a 0
);
GO


CREATE UNIQUE INDEX UX_JwtKeys_Active                                               ON dbo.JwtKeys (TenantId) WHERE IsActive = 1 AND IsDeleted = 0; 

CREATE NONCLUSTERED INDEX IX_RefreshTokens_User_Active                              ON dbo.RefreshTokens (TenantId, UserId) WHERE RevokedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_RefreshTokens_ExpiresAt                                ON dbo.RefreshTokens (TenantId, ExpiresAt) WHERE RevokedAt IS NULL;
CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_TenantId_TokenHash                 ON dbo.PasswordResetTokens (TenantId, TokenHash);
CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_RateLimit                          ON dbo.PasswordResetTokens (TenantId, UserId, AddedOn);
CREATE NONCLUSTERED INDEX IX_RolePermissions_Lookup                                 ON dbo.RolePermissions (TenantId, AppId, RoleId, ResourceId, ActionId) INCLUDE (Id);
CREATE NONCLUSTERED INDEX IX_Services_Category_Active                               ON dbo.JobDefinitions(Category, IsActive, IsDeleted);
CREATE NONCLUSTERED INDEX IX_Services_Active_System                                 ON dbo.JobDefinitions(IsActive, IsSystemJob) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Services_HangfireJobId                                 ON dbo.JobDefinitions(HangfireJobId) WHERE HangfireJobId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_UserRoles_User_App                                     ON dbo.UserRoles (TenantId, UserId, AppId) INCLUDE (RoleId);
CREATE NONCLUSTERED INDEX IX_Users_NormalizedLoginIdentifier                        ON dbo.Users (TenantId, NormalizedLoginIdentifier) INCLUDE (Id, IsActive) WHERE IsDeleted = 0;

GO

/* =========================
   ROW LEVEL SECURITY
   ========================= */

-- A função de segurança que implementa a lógica de acesso por Tenant
CREATE FUNCTION dbo.fn_TenantAccessPredicate (
    @TenantId INT
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
    SELECT 1 AS fn_access
    WHERE 
        -- SuperAdmin tem acesso a tudo (bypass RLS)
        ISNULL(CAST(SESSION_CONTEXT(N'IsSuperAdmin') AS INT), 0) = 1
        OR
        -- Verifica se o TenantId da sessão é igual ao TenantId da linha, garantindo que o usuário só veja os dados do seu próprio tenant
        (
            SESSION_CONTEXT(N'TenantId') IS NOT NULL
            AND @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS INT)
        );
GO

-- A policy de segurança que aplica a função de acesso a todas as tabelas relevantes
CREATE SECURITY POLICY dbo.TenantSecurityPolicy												                    -- Cria a policy de segurança
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Apps,					                    -- Filtro por TenantId, aplica RLS em Users
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Users,					                    -- Filtro por TenantId, aplica RLS em Users
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Roles,					                    -- Aplica RLS em Roles
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Resources,					                -- Aplica RLS em Resources
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Actions,					                    -- Aplica RLS em Actions
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RolePermissions,			                    -- Aplica RLS em RolePermissions
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.UserRoles,				                    -- Aplica RLS em UserRoles
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RefreshTokens,					            -- Aplica RLS em RefreshTokens
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.PasswordResetTokens,                            -- Aplica RLS em PasswordResetTokens
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.JwtKeys,					                    -- Aplica RLS em JwtKeys

ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Apps AFTER INSERT,	                        -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Apps AFTER UPDATE,	                        -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Apps BEFORE DELETE,                           -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Users AFTER INSERT,	                        -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Users AFTER UPDATE,	                        -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Users BEFORE DELETE,                          -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Roles AFTER INSERT,                           -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Roles AFTER UPDATE,                           -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Roles BEFORE DELETE,                          -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Resources AFTER INSERT,	                    -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Resources AFTER UPDATE,	                    -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Resources BEFORE DELETE,                      -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Actions AFTER INSERT,                         -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Actions AFTER UPDATE,                         -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.Actions BEFORE DELETE,                        -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RolePermissions AFTER INSERT,                 -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RolePermissions AFTER UPDATE,                 -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RolePermissions BEFORE DELETE,                -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.UserRoles AFTER INSERT,                       -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.UserRoles AFTER UPDATE,                       -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.UserRoles BEFORE DELETE,                      -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RefreshTokens AFTER INSERT,                   -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RefreshTokens AFTER UPDATE,                   -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.RefreshTokens BEFORE DELETE,                  -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.PasswordResetTokens AFTER INSERT,             -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.PasswordResetTokens AFTER UPDATE,             -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.PasswordResetTokens BEFORE DELETE,            -- Bloqueia DELETE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.JwtKeys AFTER INSERT,                         -- Bloqueia INSERT fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.JwtKeys AFTER UPDATE,                         -- Bloqueia UPDATE fora do Tenant
ADD BLOCK PREDICATE dbo.fn_TenantAccessPredicate(TenantId) ON dbo.JwtKeys BEFORE DELETE                         -- Bloqueia DELETE fora do Tenant

WITH (STATE = ON);																		                        -- Ativa a policy
GO