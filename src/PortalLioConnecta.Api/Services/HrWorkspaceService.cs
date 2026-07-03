using PortalLioConnecta.Api.Contracts.HrProfile;
using PortalLioConnecta.Api.Infrastructure.TotvsRm;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Models;

namespace PortalLioConnecta.Api.Services;

public class HrWorkspaceService : IHrWorkspaceService
{
    private const string Provider = "TOTVS RM";

    private readonly HrRmAccessGuard _accessGuard;
    private readonly ITotvsRmTimesheetRepository _totvsRmTimesheetRepository;
    private readonly ITotvsRmEmployeeRepository _employeeRepository;
    private readonly ITotvsRmPayrollRepository _payrollRepository;
    private readonly ITotvsRmVacationRepository _vacationRepository;
    private readonly ITotvsRmBenefitsRepository _benefitsRepository;
    private readonly ITotvsRmTeamRepository _teamRepository;
    private readonly TimesheetMergeService _timesheetMergeService;

    public HrWorkspaceService(
        HrRmAccessGuard accessGuard,
        ITotvsRmTimesheetRepository totvsRmTimesheetRepository,
        ITotvsRmEmployeeRepository employeeRepository,
        ITotvsRmPayrollRepository payrollRepository,
        ITotvsRmVacationRepository vacationRepository,
        ITotvsRmBenefitsRepository benefitsRepository,
        ITotvsRmTeamRepository teamRepository,
        TimesheetMergeService timesheetMergeService)
    {
        _accessGuard = accessGuard;
        _totvsRmTimesheetRepository = totvsRmTimesheetRepository;
        _employeeRepository = employeeRepository;
        _payrollRepository = payrollRepository;
        _vacationRepository = vacationRepository;
        _benefitsRepository = benefitsRepository;
        _teamRepository = teamRepository;
        _timesheetMergeService = timesheetMergeService;
    }

    public async Task<HrVacationResponse> GetVacationAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Ferias,
            "Consulta de ferias temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return new HrVacationResponse(
                "Ferias (Consultar/Solicitar)",
                null,
                [],
                false,
                Provider,
                false,
                resolution.AvailabilityStatus,
                resolution.UserMessage);
        }

        try
        {
            var chapa = resolution.Context!.Chapa;
            var balance = await _vacationRepository.GetBalanceAsync(chapa, cancellationToken);
            var periods = await _vacationRepository.GetPeriodsAsync(chapa, cancellationToken);

            var balanceDto = balance is null
                ? new HrVacationBalanceDto(0, 0, 0, null)
                : new HrVacationBalanceDto(
                    balance.AvailableDays,
                    balance.ScheduledDays,
                    balance.UsedDays,
                    balance.NextAcquisitionDate);

            var requests = periods
                .Select((period, index) => new HrVacationRequestDto(
                    Guid.Parse($"a200000{index % 10}-0000-4000-8000-{period.StartDate:yyyyMMdd}"),
                    HrRmMapper.MapVacationStatus(period.StatusCode),
                    period.StartDate,
                    period.EndDate,
                    period.Days,
                    period.RequestedAt ?? period.StartDate))
                .ToList();

            return new HrVacationResponse(
                "Ferias (Consultar/Solicitar)",
                balanceDto,
                requests,
                false,
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return UnavailableVacation();
        }
    }

    public async Task<HrPayslipResponse> GetPayslipsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Holerite,
            "Consulta de holerite temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return new HrPayslipResponse(
                "Holerite",
                [],
                Provider,
                false,
                resolution.AvailabilityStatus,
                resolution.UserMessage);
        }

        try
        {
            var summaries = await _payrollRepository.GetPayslipSummariesAsync(resolution.Context!.Chapa, 24, cancellationToken);
            var items = summaries.Select(summary =>
            {
                var id = HrRmMapper.BuildPeriodId(summary.AnoComp, summary.MesComp);
                return new HrPayslipDto(
                    id,
                    HrRmMapper.BuildPeriodLabel(summary.AnoComp, summary.MesComp),
                    id,
                    summary.GrossAmount,
                    summary.NetAmount,
                    summary.PaymentDate ?? new DateTime(summary.AnoComp, summary.MesComp, 1),
                    "Disponivel");
            }).ToList();

            return new HrPayslipResponse("Holerite", items, Provider, false, "ok", null);
        }
        catch (TotvsRmIntegrationException)
        {
            return new HrPayslipResponse(
                "Holerite",
                [],
                Provider,
                false,
                "rm_unavailable",
                "Nao foi possivel consultar holerites agora. Tente novamente em alguns minutos.");
        }
    }

    public async Task<HrPayslipDetailDto?> GetPayslipDetailAsync(
        PortalUser user,
        string payslipId,
        CancellationToken cancellationToken)
    {
        if (!TryParsePeriodId(payslipId, out var anoComp, out var mesComp))
        {
            return null;
        }

        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Holerite,
            "Consulta de holerite temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return null;
        }

        try
        {
            var chapa = resolution.Context!.Chapa;
            var profile = await _employeeRepository.GetProfileByChapaAsync(chapa, cancellationToken);
            var lines = await _payrollRepository.GetPayslipLinesAsync(chapa, anoComp, mesComp, cancellationToken);
            if (lines.Count == 0)
            {
                return null;
            }

            var earnings = lines.Where(line => !line.IsDeduction)
                .Select(line => new HrPayslipLineDto(line.Code, line.Description, line.Reference, line.Amount))
                .ToList();
            var deductions = lines.Where(line => line.IsDeduction)
                .Select(line => new HrPayslipLineDto(line.Code, line.Description, line.Reference, line.Amount))
                .ToList();

            var gross = earnings.Sum(item => item.Amount);
            var net = gross - deductions.Sum(item => item.Amount);
            var periodLabel = HrRmMapper.BuildPeriodLabel(anoComp, mesComp);

            return new HrPayslipDetailDto(
                payslipId,
                periodLabel,
                payslipId,
                gross,
                net,
                new DateTime(anoComp, mesComp, DateTime.DaysInMonth(anoComp, mesComp)),
                "Disponivel",
                "LIO Tecnica",
                "—",
                "—",
                profile?.Nome ?? user.DisplayName,
                chapa,
                HrRmMapper.MaskCpf(profile?.Cpf),
                profile?.FuncaoDescricao ?? user.Title ?? "Colaborador",
                profile?.SecaoDescricao ?? user.Department ?? "—",
                HrRmMapper.FormatAdmissionDate(profile?.DataAdmissao),
                profile?.Banco ?? "—",
                profile?.Agencia ?? "—",
                profile?.Conta ?? "—",
                gross,
                gross,
                gross,
                gross * 0.08m,
                earnings,
                deductions,
                earnings.Sum(item => item.Amount),
                deductions.Sum(item => item.Amount),
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return null;
        }
    }

    public async Task<HrBenefitsResponse> GetBenefitsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Beneficios,
            "Consulta de beneficios temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return new HrBenefitsResponse(
                "Beneficios (VR/VT)",
                [],
                [],
                Provider,
                false,
                resolution.AvailabilityStatus,
                resolution.UserMessage);
        }

        try
        {
            var chapa = resolution.Context!.Chapa;
            var benefits = await _benefitsRepository.GetBenefitsAsync(chapa, cancellationToken);
            var dependents = await _benefitsRepository.GetDependentsAsync(chapa, cancellationToken) ?? [];

            var items = benefits.Select(item => new HrBenefitItemDto(
                item.Code,
                item.Label,
                item.Category,
                item.Value,
                item.Status,
                item.Details)).ToList();

            var dependentItems = dependents.Select(item => new HrDependentItemDto(
                item.Name,
                item.Relationship,
                item.Status)).ToList();

            return new HrBenefitsResponse(
                "Beneficios (VR/VT)",
                items,
                dependentItems,
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return new HrBenefitsResponse(
                "Beneficios (VR/VT)",
                [],
                [],
                Provider,
                false,
                "rm_unavailable",
                "Nao foi possivel consultar beneficios agora. Tente novamente em alguns minutos.");
        }
    }

    public Task<HrEvaluationResponse> GetEvaluationAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = user;
        _ = cancellationToken;

        return Task.FromResult(new HrEvaluationResponse(
            "Minha Avaliacao",
            "—",
            "Indisponivel",
            0m,
            "—",
            [],
            "Modulo de avaliacao ainda nao integrado ao TOTVS RM neste ambiente.",
            Provider,
            false,
            "module_disabled",
            "A avaliacao de desempenho sera disponibilizada quando o modulo RM correspondente for identificado."));
    }

    public async Task<HrPersonalDataResponse> GetPersonalDataAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Cadastro,
            "Consulta de cadastro temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return BuildPersonalDataFromPortal(user, resolution.Context?.Chapa, true, resolution);
        }

        try
        {
            var profile = await _employeeRepository.GetProfileByChapaAsync(resolution.Context!.Chapa, cancellationToken);
            if (profile is null)
            {
                return BuildPersonalDataFromPortal(user, resolution.Context.Chapa, false, resolution);
            }

            var sections = new List<HrPersonalDataSectionDto>
            {
                new("Identificacao", [
                    new HrPersonalDataFieldDto("Nome", profile.Nome ?? user.DisplayName, false),
                    new HrPersonalDataFieldDto("E-mail", user.Email ?? profile.EmailPessoal ?? "—", false),
                    new HrPersonalDataFieldDto("Matricula", profile.Chapa, false),
                    new HrPersonalDataFieldDto("CPF", HrRmMapper.MaskCpf(profile.Cpf), false)
                ]),
                new("Organizacao", [
                    new HrPersonalDataFieldDto("Cargo", profile.FuncaoDescricao ?? user.Title ?? "—", false),
                    new HrPersonalDataFieldDto("Area", profile.SecaoDescricao ?? user.Department ?? "—", false),
                    new HrPersonalDataFieldDto("Gestor", profile.GestorNome ?? user.ManagerDisplayName ?? "—", false),
                    new HrPersonalDataFieldDto("Admissao", HrRmMapper.FormatAdmissionDate(profile.DataAdmissao), false)
                ]),
                new("Contato", [
                    new HrPersonalDataFieldDto("Telefone", profile.Telefone ?? "—", false),
                    new HrPersonalDataFieldDto("Cidade/UF", HrRmMapper.FormatCityState(profile.Cidade, profile.Estado), false),
                    new HrPersonalDataFieldDto("Endereco", profile.Endereco ?? "—", false)
                ])
            };

            return new HrPersonalDataResponse(
                "Dados Cadastrais",
                sections,
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return BuildPersonalDataFromPortal(
                user,
                resolution.Context?.Chapa,
                false,
                TotvsRmHrResolution.Unavailable("Nao foi possivel consultar cadastro agora."));
        }
    }

    public async Task<HrTimesheetResponse> GetTimesheetAsync(
        PortalUser user,
        int? month,
        int? year,
        CancellationToken cancellationToken)
    {
        var (resolution, runtime) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.Ponto,
            "Consulta de ponto temporariamente indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return BuildUnavailableTimesheet(resolution.AvailabilityStatus, resolution.UserMessage!);
        }

        var (dataDe, dataAte, selectedEndMonth, selectedEndYear) = TimesheetPeriodResolver.Resolve(
            month,
            year,
            runtime.TimesheetPeriodStartDay,
            runtime.TimesheetPeriodEndDay);
        var periodOptions = TimesheetPeriodResolver
            .BuildRecentPeriodOptions(
                TimesheetPeriodResolver.DefaultRecentPeriodCount,
                runtime.TimesheetPeriodStartDay,
                runtime.TimesheetPeriodEndDay)
            .Select(item => new HrTimesheetPeriodOptionDto(item.EndMonth, item.EndYear, item.Label))
            .ToList();

        try
        {
            var chapa = resolution.Context!.Chapa;
            var punches = await _totvsRmTimesheetRepository.GetPunchesAsync(chapa, dataDe, dataAte, cancellationToken);
            var processedDays = await _totvsRmTimesheetRepository.GetProcessedDaysAsync(chapa, dataDe, dataAte, cancellationToken);
            var (summary, entries) = _timesheetMergeService.Merge(dataDe, dataAte, punches, processedDays);

            var enrichedSummary = summary is null
                ? null
                : summary with { BankHours = summary.BalanceHours };

            return new HrTimesheetResponse(
                "Ponto",
                enrichedSummary,
                entries,
                Provider,
                false,
                "ok",
                null,
                selectedEndMonth,
                selectedEndYear,
                periodOptions);
        }
        catch (TotvsRmIntegrationDisabledException)
        {
            return BuildUnavailableTimesheet("rm_disabled", "Consulta de ponto temporariamente indisponivel. Entre em contato com o RH.");
        }
        catch (TotvsRmIntegrationMisconfiguredException)
        {
            return BuildUnavailableTimesheet("rm_disabled", "Consulta de ponto temporariamente indisponivel. Entre em contato com o RH.");
        }
        catch (TotvsRmIntegrationUnavailableException)
        {
            return BuildUnavailableTimesheet("rm_unavailable", "Nao foi possivel consultar o ponto agora. Tente novamente em alguns minutos.");
        }
    }

    public async Task<HrRhSummaryDto> GetRhSummaryAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, runtime) = await _accessGuard.EnsureAsync(
            user,
            _ => true,
            "Resumo RH indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return new HrRhSummaryDto(null, null, null, null, null, Provider, false, resolution.AvailabilityStatus, resolution.UserMessage);
        }

        try
        {
            var chapa = resolution.Context!.Chapa;
            var balance = await _vacationRepository.GetBalanceAsync(chapa, cancellationToken);
            var payslips = await _payrollRepository.GetPayslipSummariesAsync(chapa, 1, cancellationToken);
            var (dataDe, dataAte, _, _) = TimesheetPeriodResolver.Resolve(
                null,
                null,
                runtime.TimesheetPeriodStartDay,
                runtime.TimesheetPeriodEndDay);
            var punches = await _totvsRmTimesheetRepository.GetPunchesAsync(chapa, dataDe, dataAte, cancellationToken);
            var processedDays = await _totvsRmTimesheetRepository.GetProcessedDaysAsync(chapa, dataDe, dataAte, cancellationToken);
            var (summary, _) = _timesheetMergeService.Merge(dataDe, dataAte, punches, processedDays);

            var lastPayslip = payslips.FirstOrDefault();
            return new HrRhSummaryDto(
                balance?.AvailableDays.ToString(),
                lastPayslip is null ? null : $"R$ {lastPayslip.NetAmount:N2}",
                lastPayslip is null ? null : HrRmMapper.BuildPeriodLabel(lastPayslip.AnoComp, lastPayslip.MesComp),
                summary?.WorkedHours,
                summary?.BalanceHours,
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return new HrRhSummaryDto(null, null, null, null, null, Provider, false, "rm_unavailable", "Resumo RH indisponivel no momento.");
        }
    }

    public async Task<HrTeamDashboardResponse> GetTeamDashboardAsync(PortalUser user, CancellationToken cancellationToken)
    {
        var (resolution, _) = await _accessGuard.EnsureAsync(
            user,
            flags => flags.TeamDashboard,
            "Dashboard de equipe indisponivel.",
            cancellationToken);

        if (!resolution.IsSuccess)
        {
            return new HrTeamDashboardResponse(
                "Minha Equipe",
                [],
                0,
                Provider,
                false,
                resolution.AvailabilityStatus,
                resolution.UserMessage);
        }

        try
        {
            var codSecao = resolution.Context?.CodSecao;
            if (string.IsNullOrWhiteSpace(codSecao))
            {
                var profile = await _employeeRepository.GetProfileByChapaAsync(resolution.Context!.Chapa, cancellationToken);
                codSecao = profile?.CodSecao;
            }

            if (string.IsNullOrWhiteSpace(codSecao))
            {
                return new HrTeamDashboardResponse(
                    "Minha Equipe",
                    [],
                    0,
                    Provider,
                    false,
                    "ok",
                    "Secao do colaborador nao identificada no RM.");
            }

            var members = await _teamRepository.GetTeamMembersAsync(codSecao, cancellationToken);
            var dtos = members.Select(member => new HrTeamMemberDto(
                member.Chapa,
                member.Name,
                member.Role,
                member.Department,
                member.Status)).ToList();

            return new HrTeamDashboardResponse(
                "Minha Equipe",
                dtos,
                dtos.Count(item => item.Status == "Ativo"),
                Provider,
                false,
                "ok",
                null);
        }
        catch (TotvsRmIntegrationException)
        {
            return new HrTeamDashboardResponse(
                "Minha Equipe",
                [],
                0,
                Provider,
                false,
                "rm_unavailable",
                "Nao foi possivel consultar a equipe agora.");
        }
    }

    private static HrPersonalDataResponse BuildPersonalDataFromPortal(
        PortalUser user,
        string? matricula,
        bool isSimulated,
        TotvsRmHrResolution resolution) =>
        new(
            "Dados Cadastrais",
            [
                new HrPersonalDataSectionDto("Identificacao", [
                    new HrPersonalDataFieldDto("Nome", user.DisplayName, false),
                    new HrPersonalDataFieldDto("E-mail", user.Email ?? "—", false),
                    new HrPersonalDataFieldDto("Matricula", matricula ?? "—", false)
                ]),
                new HrPersonalDataSectionDto("Organizacao", [
                    new HrPersonalDataFieldDto("Cargo", user.Title ?? "—", false),
                    new HrPersonalDataFieldDto("Area", user.Department ?? "—", false),
                    new HrPersonalDataFieldDto("Gestor", user.ManagerDisplayName ?? "—", false)
                ])
            ],
            Provider,
            isSimulated,
            resolution.AvailabilityStatus,
            resolution.UserMessage);

    private static HrVacationResponse UnavailableVacation() =>
        new(
            "Ferias (Consultar/Solicitar)",
            null,
            [],
            false,
            Provider,
            false,
            "rm_unavailable",
            "Nao foi possivel consultar ferias agora. Tente novamente em alguns minutos.");

    private static bool TryParsePeriodId(string payslipId, out int anoComp, out int mesComp)
    {
        anoComp = 0;
        mesComp = 0;
        var parts = payslipId.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 &&
               int.TryParse(parts[0], out anoComp) &&
               int.TryParse(parts[1], out mesComp) &&
               mesComp is >= 1 and <= 12;
    }

    private static HrTimesheetResponse BuildUnavailableTimesheet(string availabilityStatus, string userMessage) =>
        new("Ponto", null, [], Provider, false, availabilityStatus, userMessage, 0, 0, []);
}
