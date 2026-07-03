-- Valida permissoes SELECT do usuario SQL ja cadastrado (ex.: rm_readonly_voltage)
-- Execute conectado ao Corpore COM as credenciais desse usuario (nao como sa/dbo).
-- Se todas as linhas retornarem OK, NAO e necessario rodar rm-sql-grants.sql.

SET NOCOUNT ON;

DECLARE @Tables TABLE (TableName SYSNAME);
INSERT INTO @Tables VALUES
 ('PFUNC'), ('PPESSOA'), ('PSECAO'), ('PFUNCAO'),
 ('PFFINANC'), ('PEVENTO'), ('PFUFERIAS'), ('PFUFERIASPER'),
 ('PFDEPEND'), ('ABATFUN'), ('AAFHTFUN'), ('ANATUBAT'), ('ACOMPFUN');

DECLARE @TableName SYSNAME;
DECLARE @Sql NVARCHAR(400);
DECLARE @Count BIGINT;
DECLARE @Ok INT = 0;
DECLARE @Fail INT = 0;

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName FROM @Tables;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'SELECT @Count = COUNT_BIG(1) FROM dbo.' + QUOTENAME(@TableName) + N' WITH (NOLOCK)';
    BEGIN TRY
        EXEC sp_executesql @Sql, N'@Count BIGINT OUTPUT', @Count = @Count OUTPUT;
        PRINT @TableName + ': OK (' + CAST(@Count AS VARCHAR(32)) + ' registros)';
        SET @Ok = @Ok + 1;
    END TRY
    BEGIN CATCH
        PRINT @TableName + ': FALHA - ' + ERROR_MESSAGE();
        SET @Fail = @Fail + 1;
    END CATCH

    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

PRINT '---';
PRINT 'Resumo: ' + CAST(@Ok AS VARCHAR(10)) + ' OK, ' + CAST(@Fail AS VARCHAR(10)) + ' sem permissao.';
IF @Fail = 0
    PRINT 'Usuario possui SELECT nas tabelas necessarias. GRANT adicional nao e obrigatorio.';
ELSE
    PRINT 'Solicite ao DBA permissao SELECT nas tabelas com FALHA ou use rm-sql-grants.sql como referencia.';
