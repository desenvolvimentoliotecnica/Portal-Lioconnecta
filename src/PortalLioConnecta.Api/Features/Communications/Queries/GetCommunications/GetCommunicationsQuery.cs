using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;

namespace PortalLioConnecta.Api.Features.Communications.Queries.GetCommunications;

public record GetCommunicationsQuery(Guid? PortalUserId = null) : IRequest<IReadOnlyList<CommunicationDto>>;
