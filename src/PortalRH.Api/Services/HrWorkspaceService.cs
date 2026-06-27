using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class HrWorkspaceService : IHrWorkspaceService
{
    private const string Provider = "TOTVS RM";
    private const bool IsSimulated = true;

    public Task<HrVacationResponse> GetVacationAsync(PortalUser user, CancellationToken cancellationToken)
    {
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
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<HrPayslipResponse> GetPayslipsAsync(PortalUser user, CancellationToken cancellationToken)
    {
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
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<HrBenefitsResponse> GetBenefitsAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var response = new HrBenefitsResponse(
            "Beneficios (VR/VT)",
            [
                new HrBenefitItemDto("VR", "Vale Refeicao", "Alimentacao", "R$ 32,00 / dia util", "Ativo", "Credito mensal liberado todo dia 1."),
                new HrBenefitItemDto("VT", "Vale Transporte", "Mobilidade", "6% do salario base", "Ativo", "Rota cadastrada: residencia - unidade Matriz."),
                new HrBenefitItemDto("SAUDE", "Plano de Saude", "Saude", "Enfermaria nacional", "Ativo", "Titular + 2 dependentes vinculados."),
                new HrBenefitItemDto("ODONTO", "Plano Odontologico", "Saude", "Essencial", "Ativo", "Cobertura preventiva e urgencia."),
                new HrBenefitItemDto("VIDA", "Seguro de Vida", "Protecao", "2x salario anual", "Ativo", "Beneficiarios atualizados em jan/2026.")
            ],
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<HrEvaluationResponse> GetEvaluationAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var response = new HrEvaluationResponse(
            "Minha Avaliacao",
            "Ciclo 2026 - Semestre 1",
            "Em andamento",
            4.2m,
            "Acima do esperado",
            [
                new HrEvaluationCompetencyDto("Entrega e qualidade", 4, 5, "Forte"),
                new HrEvaluationCompetencyDto("Colaboracao", 5, 5, "Destaque"),
                new HrEvaluationCompetencyDto("Comunicacao", 4, 5, "Forte"),
                new HrEvaluationCompetencyDto("Inovacao", 3, 5, "Em desenvolvimento"),
                new HrEvaluationCompetencyDto("Lideranca", 4, 5, "Forte")
            ],
            "Colaborador com boa consistencia nas entregas e participacao ativa nos ritos do time.",
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<HrPersonalDataResponse> GetPersonalDataAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var response = new HrPersonalDataResponse(
            "Dados Cadastrais",
            [
                new HrPersonalDataSectionDto("Identificacao", [
                    new HrPersonalDataFieldDto("Nome completo", user.DisplayName, false),
                    new HrPersonalDataFieldDto("E-mail corporativo", user.Email ?? user.Login, false),
                    new HrPersonalDataFieldDto("Cargo", user.Title ?? "Colaborador", false),
                    new HrPersonalDataFieldDto("Departamento", user.Department ?? "Companhia", false)
                ]),
                new HrPersonalDataSectionDto("Contato", [
                    new HrPersonalDataFieldDto("Telefone celular", "(11) 98888-7766", true),
                    new HrPersonalDataFieldDto("Telefone emergencia", "(11) 97777-6655", true),
                    new HrPersonalDataFieldDto("E-mail pessoal", "colaborador.pessoal@email.com", true)
                ]),
                new HrPersonalDataSectionDto("Endereco", [
                    new HrPersonalDataFieldDto("CEP", "01310-100", true),
                    new HrPersonalDataFieldDto("Logradouro", "Av. Paulista, 1000", true),
                    new HrPersonalDataFieldDto("Bairro", "Bela Vista", true),
                    new HrPersonalDataFieldDto("Cidade/UF", "Sao Paulo / SP", true)
                ])
            ],
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }

    public Task<HrTimesheetResponse> GetTimesheetAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var response = new HrTimesheetResponse(
            "Ponto",
            new HrTimesheetSummaryDto("Junho/2026", "152h32", "160h00", "+8h15", 0, 1),
            [
                new HrTimesheetEntryDto(new DateTime(2026, 6, 23), "Segunda", "08:02", "17:31", "60", "8h29", "+0h29", "Regular"),
                new HrTimesheetEntryDto(new DateTime(2026, 6, 24), "Terca", "08:11", "17:45", "60", "8h34", "+0h34", "Regular"),
                new HrTimesheetEntryDto(new DateTime(2026, 6, 25), "Quarta", "08:00", "17:28", "60", "8h28", "+0h28", "Regular"),
                new HrTimesheetEntryDto(new DateTime(2026, 6, 26), "Quinta", "08:18", "17:36", "60", "8h18", "+0h18", "Regular"),
                new HrTimesheetEntryDto(new DateTime(2026, 6, 27), "Sexta", "08:05", "16:58", "60", "7h53", "-0h07", "Saida antecipada")
            ],
            Provider,
            IsSimulated);

        return Task.FromResult(response);
    }
}
