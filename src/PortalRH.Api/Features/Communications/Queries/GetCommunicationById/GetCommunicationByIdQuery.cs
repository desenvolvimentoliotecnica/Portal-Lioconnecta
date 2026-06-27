using MediatR;
using PortalRH.Api.Contracts.Communications;

namespace PortalRH.Api.Features.Communications.Queries.GetCommunicationById;

public record GetCommunicationByIdQuery(Guid Id, Guid? PortalUserId = null) : IRequest<CommunicationDto?>;
