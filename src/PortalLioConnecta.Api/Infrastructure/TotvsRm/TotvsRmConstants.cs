namespace PortalLioConnecta.Api.Infrastructure.TotvsRm;

public static class TotvsRmConstants
{
    public const short CodColigada = 1;
    public const int ChapaLength = 8;
    public const string PunchTableName = "ABATFUN";
    public const string ProcessedDayTableName = "AAFHTFUN";
    public const string EmployeeTableName = "PFUNC";

    /// <summary>
    /// Tabelas consultadas pela integracao read-only do portal.
    /// Usado pelo teste de conexao para validar permissoes do usuario SQL ja cadastrado.
    /// </summary>
    public static readonly string[] RequiredReadTables =
    [
        "PFUNC",
        "PPESSOA",
        "PSECAO",
        "PFUNCAO",
        "PFFINANC",
        "PEVENTO",
        "PFPERFF",
        "PFUFERIAS",
        "PFUFERIASPER",
        "PFDEPEND",
        "ABATFUN",
        "AAFHTFUN",
        "ANATUBAT",
        "ACOMPFUN",
        "ASALDOBANCOHOR"
    ];
}
