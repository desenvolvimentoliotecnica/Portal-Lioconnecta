using Microsoft.EntityFrameworkCore;
using PortalRH.Api.Contracts.QuickLinks;
using PortalRH.Api.Data;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class QuickLinkService : IQuickLinkService
{
    private readonly PortalRhDbContext _dbContext;

    public QuickLinkService(PortalRhDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<QuickLinkListResponse> GetActiveAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.QuickLinks
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Label)
            .Select(item => new QuickLinkDto(item.Id, item.Label, item.ShortLabel, item.ClassName, item.Url))
            .ToListAsync(cancellationToken);

        return new QuickLinkListResponse(items);
    }

    public async Task EnsureSeedAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.QuickLinks.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var seedItems = new (string Label, string ShortLabel, string ClassName, string Url, int SortOrder)[]
        {
            ("Gestao Integrada", "SAP", "sap", "https://portal.example.local/sap", 1),
            ("Google Workspace", "G", "google", "https://workspace.google.com", 2),
            ("ServiceNow", "SN", "service", "https://portal.example.local/servicenow", 3),
            ("Microsoft Teams", "T", "teams", "https://teams.microsoft.com", 4),
            ("e-Learning Treinamentos", "EL", "learn", "https://portal.example.local/lms", 5),
            ("Jira/Confluence", "JC", "jira", "https://portal.example.local/jira", 6),
            ("Ferias", "FR", "sap", "https://portal.example.local/rh/ferias", 7),
            ("Holerite", "HL", "google", "https://portal.example.local/rh/holerite", 8),
            ("Beneficios", "BF", "teams", "https://portal.example.local/rh/beneficios", 9),
            ("Ponto", "PT", "learn", "https://portal.example.local/rh/ponto", 10)
        };

        foreach (var item in seedItems)
        {
            _dbContext.QuickLinks.Add(new QuickLink
            {
                Id = Guid.NewGuid(),
                Label = item.Label,
                ShortLabel = item.ShortLabel,
                ClassName = item.ClassName,
                Url = item.Url,
                SortOrder = item.SortOrder,
                IsActive = true,
                Audience = "Toda a companhia",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
