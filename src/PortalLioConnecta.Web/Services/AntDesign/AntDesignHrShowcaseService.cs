using PortalLioConnecta.Web.Models.AntDesignShowcase;

namespace PortalLioConnecta.Web.Services.AntDesign;

public class AntDesignHrShowcaseService : IAntDesignHrShowcaseService
{
    public AntDesignHrShowcaseViewModel GetShowcase()
    {
        return new AntDesignHrShowcaseViewModel
        {
            Title = "Showcase Ant Design Blazor",
            Subtitle = "Visão mockada de um portal de RH com foco em recrutamento e seleção.",
            Metrics =
            [
                new AntDesignMetricViewModel { Title = "Vagas abertas", Value = "18", AccentColor = "blue" },
                new AntDesignMetricViewModel { Title = "Candidatos ativos", Value = "126", AccentColor = "green" },
                new AntDesignMetricViewModel { Title = "Tempo médio", Value = "24 dias", AccentColor = "gold" },
                new AntDesignMetricViewModel { Title = "Admissões no mês", Value = "7", AccentColor = "volcano" }
            ],
            SummaryCards =
            [
                new AntDesignSummaryCardViewModel
                {
                    Title = "Hiring sprint",
                    Description = "Sprint ativa para vagas críticas de tecnologia e RH.",
                    TagText = "Em andamento",
                    TagColor = "processing"
                },
                new AntDesignSummaryCardViewModel
                {
                    Title = "SLA de triagem",
                    Description = "Primeira resposta em até 48 horas para novos candidatos.",
                    TagText = "48h",
                    TagColor = "blue"
                },
                new AntDesignSummaryCardViewModel
                {
                    Title = "Pacote RM",
                    Description = "Checklist de integração pronto para sincronização com TOTVS RM.",
                    TagText = "Pronto",
                    TagColor = "green"
                }
            ],
            Filters =
            [
                new AntDesignFilterChipViewModel { Label = "RH", Color = "blue" },
                new AntDesignFilterChipViewModel { Label = "Tecnologia", Color = "purple" },
                new AntDesignFilterChipViewModel { Label = "Alta prioridade", Color = "red" },
                new AntDesignFilterChipViewModel { Label = "TOTVS RM", Color = "gold" }
            ],
            Candidates =
            [
                new AntDesignCandidateViewModel { Name = "Mariana Costa", Position = "Analista de RH", Stage = "Entrevista final", Score = 92, TagColor = "green" },
                new AntDesignCandidateViewModel { Name = "Rafael Souza", Position = "Desenvolvedor .NET", Stage = "Teste técnico", Score = 85, TagColor = "blue" },
                new AntDesignCandidateViewModel { Name = "Camila Nogueira", Position = "Business Partner", Stage = "Triagem", Score = 74, TagColor = "gold" },
                new AntDesignCandidateViewModel { Name = "Lucas Martins", Position = "Coordenador de Folha", Stage = "Proposta", Score = 96, TagColor = "purple" }
            ],
            TimelineEvents =
            [
                new AntDesignTimelineEventViewModel { DateLabel = "11 Jun", Description = "Vaga de Coordenador de RH publicada no portal interno.", Color = "blue" },
                new AntDesignTimelineEventViewModel { DateLabel = "09 Jun", Description = "Integração mockada com TOTVS RM revisada com o time funcional.", Color = "green" },
                new AntDesignTimelineEventViewModel { DateLabel = "07 Jun", Description = "Fluxo de aprovação de vaga validado pelo gestor da área.", Color = "gold" },
                new AntDesignTimelineEventViewModel { DateLabel = "05 Jun", Description = "Protótipo visual do dashboard de recrutamento aprovado.", Color = "red" }
            ],
            Vacancies =
            [
                new AntDesignVacancyViewModel { Title = "Analista de Recrutamento", Department = "RH", HiringManager = "Paula Mendes", Status = "Em triagem", StatusColor = "blue" },
                new AntDesignVacancyViewModel { Title = "Desenvolvedor Backend .NET", Department = "Tecnologia", HiringManager = "João Ribeiro", Status = "Entrevistas", StatusColor = "gold" },
                new AntDesignVacancyViewModel { Title = "Assistente de DP", Department = "Departamento Pessoal", HiringManager = "Fernanda Lima", Status = "Proposta", StatusColor = "green" }
            ],
            Approvals =
            [
                new AntDesignApprovalViewModel { RequestTitle = "Abertura - Coordenador de RH", Owner = "Paula Mendes", Priority = "Alta", PriorityColor = "red", Eta = "Hoje" },
                new AntDesignApprovalViewModel { RequestTitle = "Proposta - Desenvolvedor .NET", Owner = "João Ribeiro", Priority = "Média", PriorityColor = "gold", Eta = "Amanhã" },
                new AntDesignApprovalViewModel { RequestTitle = "Admissão - Assistente de DP", Owner = "Fernanda Lima", Priority = "Alta", PriorityColor = "red", Eta = "Hoje" }
            ],
            IntegrationCheckpoints =
            [
                new AntDesignIntegrationCheckpointViewModel { Title = "Mapa de campos RM", Detail = "Validação dos identificadores de vaga, candidato e admissão.", Done = true },
                new AntDesignIntegrationCheckpointViewModel { Title = "Webhook de status", Detail = "Atualização automática de etapa do processo seletivo.", Done = true },
                new AntDesignIntegrationCheckpointViewModel { Title = "Carga salarial", Detail = "Mock de faixa salarial para timeline de carreira.", Done = false },
                new AntDesignIntegrationCheckpointViewModel { Title = "Pacote documental", Detail = "Checklist documental pronto para envio ao candidato.", Done = false }
            ],
            Activities =
            [
                new AntDesignActivityViewModel { Title = "Triagem concluída", Description = "11 candidatos priorizados para a vaga de Analista de RH.", TimeLabel = "Há 15 min" },
                new AntDesignActivityViewModel { Title = "Entrevista agendada", Description = "Painel final com gestor técnico para Rafael Souza.", TimeLabel = "Há 42 min" },
                new AntDesignActivityViewModel { Title = "Aprovação liberada", Description = "Budget aprovado para a vaga de Coordenador de RH.", TimeLabel = "Há 1 hora" },
                new AntDesignActivityViewModel { Title = "Checklist criado", Description = "Admissão da Camila Nogueira pronta para a próxima etapa.", TimeLabel = "Há 2 horas" }
            ],
            Stages =
            [
                new AntDesignStageViewModel { Title = "Abertura", Subtitle = "Solicitação validada", Percent = 100, Status = "Success" },
                new AntDesignStageViewModel { Title = "Triagem", Subtitle = "Candidatos priorizados", Percent = 78, Status = "Active" },
                new AntDesignStageViewModel { Title = "Entrevistas", Subtitle = "Agenda em andamento", Percent = 52, Status = "Normal" },
                new AntDesignStageViewModel { Title = "Oferta", Subtitle = "Proposta em negociação", Percent = 26, Status = "Exception" }
            ]
        };
    }
}
