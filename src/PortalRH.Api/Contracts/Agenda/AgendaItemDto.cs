namespace PortalRH.Api.Contracts.Agenda;

public record AgendaItemDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    string TimeLabel,
    string Source,
    string Audience,
    string? JoinUrl,
    IReadOnlyList<AgendaParticipantDto> Participants,
    DateTime StartAtUtc,
    DateTime EndAtUtc);
