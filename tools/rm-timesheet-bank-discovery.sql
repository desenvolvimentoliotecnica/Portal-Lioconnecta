-- Descoberta de KPIs de banco de horas (Saldo Anterior / Saldo do Periodo / Total Banco)
-- Execute no SQL Server conectado ao Corpore.
-- Funcionario exemplo: CHAPA 00000887 (Leonardo Sabino Mendes)
-- Periodo exemplo: 16/06/2026 a 15/07/2026

DECLARE @CodColigada SMALLINT = 1;
DECLARE @Chapa VARCHAR(16) = '00000887';
DECLARE @DataDe DATE = '2026-06-16';
DECLARE @DataAte DATE = '2026-07-15';

-- 1) Colunas das tabelas oficiais de banco de horas (TOTVS)
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN ('ACOMPFUN', 'ABANCOHORFUNDETALHE', 'ASALDOBANCOHOR', 'ABANCOHORFUN', 'AAFHTFUN')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- 2) Registros brutos ACOMPFUN (saldo por periodo)
SELECT TOP 20 *
FROM dbo.ACOMPFUN WITH (NOLOCK)
WHERE CODCOLIGADA = @CodColigada
  AND CHAPA = @Chapa
ORDER BY 1 DESC;

-- 3) ACOMPFUN filtrado pelo periodo 16/06 a 15/07
SELECT TOP 5 *
FROM dbo.ACOMPFUN WITH (NOLOCK)
WHERE CODCOLIGADA = @CodColigada
  AND CHAPA = @Chapa
  AND TRY_CONVERT(date, INICIOPER) = @DataDe
  AND TRY_CONVERT(date, FIMPER) = @DataAte;

-- 4) ABANCOHORFUNDETALHE (guia Banco de Horas no espelho)
SELECT TOP 20 *
FROM dbo.ABANCOHORFUNDETALHE WITH (NOLOCK)
WHERE CODCOLIGADA = @CodColigada
  AND CHAPA = @Chapa
ORDER BY 1 DESC;

-- 5) ASALDOBANCOHOR (saldo sintetico do espelho — fonte dos KPIs)
SELECT TOP 20 *
FROM dbo.ASALDOBANCOHOR WITH (NOLOCK)
WHERE CODCOLIGADA = @CodColigada
  AND CHAPA = @Chapa
ORDER BY FIMPER DESC;

-- 5b) KPIs calculados (referencia: +17:40 / +12:06 / +29:46)
SELECT
    COALESCE(EXTRAANT, 0) - COALESCE(ATRASOANT, 0) - COALESCE(FALTAANT, 0) AS SaldoAnteriorMin,
    COALESCE(EXTRAATU, 0) - COALESCE(ATRASOATU, 0) - COALESCE(FALTAATU, 0) AS SaldoPeriodoMin,
    (COALESCE(EXTRAANT, 0) - COALESCE(ATRASOANT, 0) - COALESCE(FALTAANT, 0))
      + (COALESCE(EXTRAATU, 0) - COALESCE(ATRASOATU, 0) - COALESCE(FALTAATU, 0)) AS TotalBancoMin
FROM dbo.ASALDOBANCOHOR WITH (NOLOCK)
WHERE CODCOLIGADA = @CodColigada
  AND CHAPA = @Chapa
  AND TRY_CONVERT(date, INICIOPER) = @DataDe
  AND TRY_CONVERT(date, FIMPER) = @DataAte;

-- 6) Conferencia diaria AAFHTFUN no periodo (inclui ABONO e EXTRAAUTORIZADO)
SELECT
    CAST(H.DATA AS DATE) AS DataPonto,
    H.HTRAB,
    H.ABONO,
    H.COMPENSADO,
    H.EXTRAAUTORIZADO,
    H.BASE,
    H.TEMPOREF,
    COALESCE(H.HTRAB, 0) + COALESCE(H.ABONO, 0) + COALESCE(H.COMPENSADO, 0) - COALESCE(H.BASE, H.TEMPOREF, 0) AS SaldoCalculadoMin,
    H.ATRASO,
    H.FALTA,
    H.ATRASOCALC,
    H.FALTACALC
FROM dbo.AAFHTFUN H WITH (NOLOCK)
WHERE H.CODCOLIGADA = @CodColigada
  AND H.CHAPA = @Chapa
  AND CAST(H.DATA AS DATE) BETWEEN @DataDe AND @DataAte
ORDER BY H.DATA;

-- 7) Totais agregados do espelho (referencia rapida)
SELECT
    SUM(COALESCE(H.HTRAB, 0)) AS TotalHTrabMin,
    SUM(COALESCE(H.BASE, H.TEMPOREF, 0)) AS TotalBaseMin,
    SUM(COALESCE(H.HTRAB, 0) - COALESCE(H.BASE, H.TEMPOREF, 0)) AS SaldoCalculadoMin
FROM dbo.AAFHTFUN H WITH (NOLOCK)
WHERE H.CODCOLIGADA = @CodColigada
  AND H.CHAPA = @Chapa
  AND CAST(H.DATA AS DATE) BETWEEN @DataDe AND @DataAte;

-- Valores esperados no app RM (referencia):
-- Saldo Anterior: +17:40 (1060 min)
-- Saldo do Periodo: +12:06 (726 min)
-- Total Banco: +29:46 (1786 min)
