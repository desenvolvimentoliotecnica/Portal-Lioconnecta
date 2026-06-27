namespace PortalRH.Api.Contracts.Agenda;

public record AgendaDayResponse(
    DateOnly Date,
    int TotalCount,
    IReadOnlyList<AgendaItemDto> Items);
