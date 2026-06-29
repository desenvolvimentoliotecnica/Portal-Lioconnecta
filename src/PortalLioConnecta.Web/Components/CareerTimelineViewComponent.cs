using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Web.Models.CareerTimeline;

namespace PortalLioConnecta.Web.Components;

public class CareerTimelineViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var milestones = new[]
        {
            new TimelineMilestoneViewModel { Step = 1, DateValue = new DateTime(2021, 2, 28), DateText = "28/02/2021", EventText = "Admissão", SalaryText = "R$ 5.850,59", AccentClass = "timeline-accent-blue", Icon = TimelineIconKind.Admission },
            new TimelineMilestoneViewModel { Step = 2, DateValue = new DateTime(2021, 4, 30), DateText = "30/04/2021", EventText = "Acordo Coletivo", SalaryText = "R$ 5.967,60", AccentClass = "timeline-accent-slate", Icon = TimelineIconKind.CollectiveAgreement },
            new TimelineMilestoneViewModel { Step = 3, DateValue = new DateTime(2021, 9, 30), DateText = "30/09/2021", EventText = "Plano de Cargos e Salários", SalaryText = "R$ 5.967,61", AccentClass = "timeline-accent-purple", Icon = TimelineIconKind.SalaryPlan },
            new TimelineMilestoneViewModel { Step = 4, DateValue = new DateTime(2022, 2, 28), DateText = "28/02/2022", EventText = "Promoção", SalaryText = "R$ 6.329,00", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion },
            new TimelineMilestoneViewModel { Step = 5, DateValue = new DateTime(2022, 2, 28), DateText = "28/02/2022", EventText = "Promoção", SalaryText = "R$ 6.329,00", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion },
            new TimelineMilestoneViewModel { Step = 6, DateValue = new DateTime(2022, 4, 30), DateText = "30/04/2022", EventText = "Acordo Coletivo", SalaryText = "R$ 6.768,23", AccentClass = "timeline-accent-slate", Icon = TimelineIconKind.CollectiveAgreement },
            new TimelineMilestoneViewModel { Step = 7, DateValue = new DateTime(2023, 3, 31), DateText = "31/03/2023", EventText = "Enquadramento Salarial", SalaryText = "R$ 8.121,88", AccentClass = "timeline-accent-teal", Icon = TimelineIconKind.SalaryBracket },
            new TimelineMilestoneViewModel { Step = 8, DateValue = new DateTime(2023, 5, 31), DateText = "31/05/2023", EventText = "Enquadramento Salarial", SalaryText = "R$ 9.541,00", AccentClass = "timeline-accent-teal", Icon = TimelineIconKind.SalaryBracket },
            new TimelineMilestoneViewModel { Step = 9, DateValue = new DateTime(2023, 7, 31), DateText = "31/07/2023", EventText = "Promoção", SalaryText = "R$ 11.449,20", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion },
            new TimelineMilestoneViewModel { Step = 10, DateValue = new DateTime(2023, 9, 30), DateText = "30/09/2023", EventText = "Promoção", SalaryText = "R$ 12.137,00", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion },
            new TimelineMilestoneViewModel { Step = 11, DateValue = new DateTime(2024, 4, 30), DateText = "30/04/2024", EventText = "Acordo Coletivo", SalaryText = "R$ 12.743,85", AccentClass = "timeline-accent-slate", Icon = TimelineIconKind.CollectiveAgreement },
            new TimelineMilestoneViewModel { Step = 12, DateValue = new DateTime(2024, 5, 31), DateText = "31/05/2024", EventText = "Promoção", SalaryText = "R$ 15.232,00", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion }
        };

        var model = new CareerTimelineShowcaseViewModel
        {
            Title = "TIMELINE DA CARREIRA",
            Subtitle = "Evolução profissional e salarial",
            SummaryCards = new[]
            {
                new TimelineSummaryCardViewModel
                {
                    Title = "Salário inicial",
                    Value = "R$ 5.850,59",
                    AccentClass = "timeline-accent-blue",
                    Icon = TimelineIconKind.Promotion
                },
                new TimelineSummaryCardViewModel
                {
                    Title = "Salário atual",
                    Value = "R$ 15.232,00",
                    AccentClass = "timeline-accent-green",
                    Icon = TimelineIconKind.Promotion
                },
                new TimelineSummaryCardViewModel
                {
                    Title = "Evolução total",
                    Value = "+160,35%",
                    AccentClass = "timeline-accent-purple",
                    Icon = TimelineIconKind.Promotion
                }
            },
            Milestones = milestones,
            TrackRows = new[]
            {
                BuildTrackRow(milestones.Take(6).ToArray()),
                BuildTrackRow(milestones.Skip(6).Take(6).ToArray())
            },
            LegendItems = new[]
            {
                new TimelineLegendItemViewModel { Text = "Admissão", AccentClass = "timeline-accent-blue", Icon = TimelineIconKind.Admission },
                new TimelineLegendItemViewModel { Text = "Promoção", AccentClass = "timeline-accent-green", Icon = TimelineIconKind.Promotion },
                new TimelineLegendItemViewModel { Text = "Acordo Coletivo", AccentClass = "timeline-accent-slate", Icon = TimelineIconKind.CollectiveAgreement },
                new TimelineLegendItemViewModel { Text = "Plano de Cargos e Salários", AccentClass = "timeline-accent-purple", Icon = TimelineIconKind.SalaryPlan },
                new TimelineLegendItemViewModel { Text = "Enquadramento Salarial", AccentClass = "timeline-accent-teal", Icon = TimelineIconKind.SalaryBracket }
            }
        };

        return View(model);
    }

    private static CareerTimelineTrackRowViewModel BuildTrackRow(TimelineMilestoneViewModel[] milestones)
    {
        var gaps = new List<TimelineGapViewModel>();

        for (var i = 0; i < milestones.Length - 1; i++)
        {
            var days = (milestones[i + 1].DateValue.Date - milestones[i].DateValue.Date).Days;
            gaps.Add(new TimelineGapViewModel
            {
                Label = days == 1 ? "1 dia" : $"{days} dias"
            });
        }

        return new CareerTimelineTrackRowViewModel
        {
            Milestones = milestones,
            Gaps = gaps
        };
    }
}
