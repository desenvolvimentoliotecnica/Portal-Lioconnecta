using MediatR;
using PortalRH.Api.Contracts.Communications;

namespace PortalRH.Api.Features.Communications.Queries.GetCommunications;

public record GetCommunicationsQuery(Guid? PortalUserId = null) : IRequest<IReadOnlyList<CommunicationDto>>;
