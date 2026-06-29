namespace PortalLioConnecta.Api.Contracts.Agenda;

public sealed record AgendaParticipantDto(
    string Name,
    string Email,
    string Role,
    string ResponseStatus,
    string PhotoUrl = "");
