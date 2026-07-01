using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;

namespace PortalLioConnecta.Api.Features.Communications.Queries.GetCommunicationById;

public record GetCommunicationByIdQuery(Guid Id, Guid? PortalUserId = null) : IRequest<CommunicationDto?>;
