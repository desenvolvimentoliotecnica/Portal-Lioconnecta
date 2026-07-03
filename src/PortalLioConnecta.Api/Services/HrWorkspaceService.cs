using PortalLioConnecta.Api.Contracts.HrProfile;
using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class HrWorkspaceService : IHrWorkspaceService
{
    private const string Provider = "TOTVS RM";

    private readonly ITotvsRmConfigurationService _totvsRmConfigurationService;
    private readonly ITotvsRmTimesheetRepository _totvsRmTimesheetRepository;
    private readonly TimesheetMergeService _timesheetMergeService;
    private readonly IPortalUserEmployeeIdResolver _employeeIdResolver;

    public HrWorkspaceService(
        ITotvsRmConfigurationService totvsRmConfigurationService,
        ITotvsRmTimesheetRepository totvsRmTimesheetRepository,
        TimesheetMergeService timesheetMergeService,
        IPortalUserEmployeeIdResolver employeeIdResolver)
    {
        _totvsRmConfigurationService = totvsRmConfigurationService;
        _totvsRmTimesheetRepository = totvsRmTimesheetRepository;
        _timesheetMergeService = timesheetMergeService;
        _employeeIdResolver = employeeIdResolver;
    }

    public Task<HrVacationResponse> GetVacationAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var now = DateTime.UtcNow;
        var response = new HrVacationResponse(
            "Ferias (Consultar/Solicitar)",
            new HrVacationBalanceDto(18, 5, 12, now.AddMonths(2)),
            [
                new HrVacationRequestDto(
                    Guid.Parse("a1000001-0000-4000-8000-000000000001"),
                    "Aprovado",
                    new DateTime(2026, 7, 14),
                    new DateTime(2026, 7, 25),
                    10,
                    now.AddMonths(-1)),
                new HrVacationRequestDto(
                    Guid.Parse("a1000001-0000-4000-8000-000000000002"),
                    "Em analise",
                    new DateTime(2026, 12, 22),
                    new DateTime(2027, 1, 5),
                    10,
                    now.AddDays(-3))
            ],
            true,
            Provider,
            true);

        return Task.FromResult(response);
    }

    public Task<HrPayslipResponse> GetPayslipsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new HrPayslipResponse(
            "Holerite",
            [
                new HrPayslipDto("2026-05", "Maio/2026", "2026-05", 8420.55m, 6234.18m, new DateTime(2026, 5, 30), "Disponivel"),
                new HrPayslipDto("2026-04", "Abril/2026", "2026-04", 8420.55m, 6188.42m, new DateTime(2026, 4, 30), "Disponivel"),
                new HrPayslipDto("2026-03", "Marco/2026", "2026-03", 8420.55m, 6201.07m, new DateTime(2026, 3, 31), "Disponivel"),
                new HrPayslipDto("2026-02", "Fevereiro/2026", "2026-02", 8420.55m, 6195.33m, new DateTime(2026, 2, 28), "Disponivel")
            ],
            Provider,
            true);

        return Task.FromResult(response);
    }

    public Task<HrPayslipDetailDto?> GetPayslipDetailAsync(PortalUser user, string payslipId, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var summaries = new Dictionary<string, (string PeriodLabel, string ReferenceMonth, decimal Gross, decimal Net, DateTime PaymentDate)>(StringComparer.OrdinalIgnoreCase)
        {
            ["2026-05"] = ("Maio/2026", "2026-05", 8420.55m, 6234.18m, new DateTime(2026, 5, 30)),
            ["2026-04"] = ("Abril/2026", "2026-04", 8420.55m, 6188.42m, new DateTime(2026, 4, 30)),
            ["2026-03"] = ("Marco/2026", "2026-03", 8420.55m, 6201.07m, new DateTime(2026, 3, 31)),
            ["2026-02"] = ("Fevereiro/2026", "2026-02", 8420.55m, 6195.33m, new DateTime(2026, 2, 28))
        };

        if (!summaries.TryGetValue(payslipId, out var summary))
        {
            return Task.FromResult<HrPayslipDetailDto?>(null);
        }

        var earnings = new List<HrPayslipLineDto>
        {
            new("001", "Salario base", "30,00 d", 7200.00m),
            new("020", "Horas extras 50%", "8,00 h", 480.55m),
            new("035", "Adicional noturno", "12,00 h", 240.00m),
            new("050", "Premio produtividade", "—", 500.00m)
        };

        var deductions = new List<HrPayslipLineDto>
        {
            new("101", "INSS", "11,00 %", 924.26m),
            new("102", "IRRF", "15,00 %", 612.11m),
            new("120", "Vale transporte", "6,00 %", 432.00m),
            new("130", "Plano de saude", "—", 218.00m)
        };

        var response = new HrPayslipDetailDto(
            payslipId,
            summary.PeriodLabel,
            summary.ReferenceMonth,
            summary.Gross,
            summary.Net,
            summary.PaymentDate,
            "Disponivel",
            "LIOCONNECTA Tecnologia LTDA",
            "12.345.678/0001-90",
            "Av. Paulista, 1000 - Bela Vista - Sao Paulo/SP",
            user.DisplayName,
            "00012345",
            "123.456.789-00",
            user.Title ?? "Colaborador",
            user.Department ?? "—",
            "15/03/2019",
            "Banco Itau Unibanco S.A.",
            "1234",
            "56789-0",
            7200.00m,
            summary.Gross,
            summary.Gross,
            summary.Gross * 0.08m,
            earnings,
            deductions,
            earnings.Sum(item => item.Amount),
            deductions.Sum(item => item.Amount),
            Provider,
            true);

        return Task.FromResult<HrPayslipDetailDto?>(response);
    }

    public Task<HrBenefitsResponse> GetBenefitsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new HrBenefitsResponse(
            "Beneficios (VR/VT)",
            [
                new HrBenefitItemDto("VR", "Vale Refeicao", "Alimentacao", "R$ 32,00/dia", "Ativo", "Credito diario em cartao"),
                new HrBenefitItemDto("VT", "Vale Transporte", "Mobilidade", "6% do salario", "Ativo", "Integracao com cartao corporativo"),
                new HrBenefitItemDto("PS", "Plano de Saude", "Saude", "Coparticipacao", "Ativo", "Titular + dependentes")
            ],
            Provider,
            true);

        return Task.FromResult(response);
    }

    public Task<HrEvaluationResponse> GetEvaluationAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        var response = new HrEvaluationResponse(
            "Minha Avaliacao",
            "Ciclo 2026.1",
            "Concluida",
            4.2m,
            "Acima da media",
            [
                new HrEvaluationCompetencyDto("Colaboracao", 4, 5, "Consistente"),
                new HrEvaluationCompetencyDto("Entrega", 5, 5, "Excelente"),
                new HrEvaluationCompetencyDto("Comunicacao", 4, 5, "Consistente"),
                new HrEvaluationCompetencyDto("Inovacao", 4, 5, "Consistente")
            ],
            "Demonstra ownership e apoia o time em iniciativas transversais.",
            Provider,
            true);

        return Task.FromResult(response);
    }

    public async Task<HrPersonalDataResponse> GetPersonalDataAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var resolution = await _employeeIdResolver.ResolveAsync(user, persistWhenFound: true, cancellationToken);

        var response = new HrPersonalDataResponse(
            "Dados Cadastrais",
            [
                new HrPersonalDataSectionDto("Identificacao", [
                    new HrPersonalDataFieldDto("Nome", user.DisplayName, false),
                    new HrPersonalDataFieldDto("E-mail", user.Email ?? "—", false),
                    new HrPersonalDataFieldDto("Matricula", resolution.EmployeeId ?? "—", false)
                ]),
                new HrPersonalDataSectionDto("Organizacao", [
                    new HrPersonalDataFieldDto("Cargo", user.Title ?? "—", false),
                    new HrPersonalDataFieldDto("Area", user.Department ?? "—", false),
                    new HrPersonalDataFieldDto("Gestor", user.ManagerDisplayName ?? "—", false)
                ]),
                new HrPersonalDataSectionDto("Contato", [
                    new HrPersonalDataFieldDto("Telefone", "(11) 99999-0000", true),
                    new HrPersonalDataFieldDto("Cidade/UF", "Sao Paulo / SP", true)
                ])
            ],
            Provider,
            true);

        return response;
    }

    public async Task<HrTimesheetResponse> GetTimesheetAsync(
        PortalUser user,
        int? month,
        int? year,
        CancellationToken cancellationToken)
    {
        var (dataDe, dataAte) = ResolvePeriod(month, year);
        var resolution = await _employeeIdResolver.ResolveAsync(user, persistWhenFound: true, cancellationToken);

        if (string.IsNullOrWhiteSpace(resolution.EmployeeId))
        {
            return BuildUnavailableResponse(
                "missing_employee_id",
                PortalUserEmployeeIdResolution.BuildMissingProfileMessage(resolution.MessageLabel));
        }

        var runtime = await _totvsRmConfigurationService.GetRuntimeConfigurationAsync(cancellationToken);
        if (!runtime.IsEnabled)
        {
            return BuildUnavailableResponse(
                "rm_disabled",
                "Consulta de ponto temporariamente indisponivel. Entre em contato com o RH.");
        }

        var chapa = TotvsRmChapaNormalizer.Normalize(resolution.EmployeeId);
        if (string.IsNullOrWhiteSpace(chapa))
        {
            return BuildUnavailableResponse(
                "missing_employee_id",
                PortalUserEmployeeIdResolution.BuildMissingProfileMessage(resolution.EmployeeId));
        }

        try
        {
            var punches = await _totvsRmTimesheetRepository.GetPunchesAsync(chapa, dataDe, dataAte, cancellationToken);
            var processedDays = await _totvsRmTimesheetRepository.GetProcessedDaysAsync(chapa, dataDe, dataAte, cancellationToken);
            var (summary, entries) = _timesheetMergeService.Merge(dataDe, dataAte, punches, processedDays);

            return new HrTimesheetResponse(
                "Ponto",
                summary,
                entries,
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationDisabledException)
        {
            return BuildUnavailableResponse(
                "rm_disabled",
                "Consulta de ponto temporariamente indisponivel. Entre em contato com o RH.");
        }
        catch (TotvsRmIntegrationMisconfiguredException)
        {
            return BuildUnavailableResponse(
                "rm_disabled",
                "Consulta de ponto temporariamente indisponivel. Entre em contato com o RH.");
        }
        catch (TotvsRmIntegrationUnavailableException)
        {
            return BuildUnavailableResponse(
                "rm_unavailable",
                "Nao foi possivel consultar o ponto agora. Tente novamente em alguns minutos.");
        }
    }

    private static (DateTime DataDe, DateTime DataAte) ResolvePeriod(int? month, int? year)
    {
        var now = DateTime.UtcNow;
        var resolvedYear = year is >= 2000 and <= 2100 ? year.Value : now.Year;
        var resolvedMonth = month is >= 1 and <= 12 ? month.Value : now.Month;

        var dataDe = new DateTime(resolvedYear, resolvedMonth, 1);
        var dataAte = dataDe.AddMonths(1).AddDays(-1);
        return (dataDe, dataAte);
    }

    private static HrTimesheetResponse BuildUnavailableResponse(string availabilityStatus, string userMessage)
    {
        return new HrTimesheetResponse(
            "Ponto",
            null,
            [],
            Provider,
            false,
            availabilityStatus,
            userMessage);
    }
}
