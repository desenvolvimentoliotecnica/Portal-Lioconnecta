-- Referencia opcional: GRANTs para NOVO usuario SQL sem permissoes no Corpore.
-- Se voce ja usa um login read-only provisionado (ex.: rm_readonly_voltage),
-- NAO execute este script. Valide antes com tools/rm-validate-permissions.sql
-- ou pelo botao "Testar conexao" em #configuracoes/totvs-rm no portal.

USE [Corpore];
GO

DECLARE @Login SYSNAME = N'portal_rm_read';

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @Login)
BEGIN
    CREATE USER [portal_rm_read] FOR LOGIN [portal_rm_read];
END
GO

GRANT SELECT ON dbo.PFUNC TO [portal_rm_read];
GRANT SELECT ON dbo.PPESSOA TO [portal_rm_read];
GRANT SELECT ON dbo.PSECAO TO [portal_rm_read];
GRANT SELECT ON dbo.PFUNCAO TO [portal_rm_read];
GRANT SELECT ON dbo.PFFINANC TO [portal_rm_read];
GRANT SELECT ON dbo.PEVENTO TO [portal_rm_read];
GRANT SELECT ON dbo.PFUFERIAS TO [portal_rm_read];
GRANT SELECT ON dbo.PFUFERIASPER TO [portal_rm_read];
GRANT SELECT ON dbo.PFDEPEND TO [portal_rm_read];
GRANT SELECT ON dbo.ABATFUN TO [portal_rm_read];
GRANT SELECT ON dbo.AAFHTFUN TO [portal_rm_read];
GRANT SELECT ON dbo.ANATUBAT TO [portal_rm_read];
GRANT SELECT ON dbo.ACOMPFUN TO [portal_rm_read];
GO
