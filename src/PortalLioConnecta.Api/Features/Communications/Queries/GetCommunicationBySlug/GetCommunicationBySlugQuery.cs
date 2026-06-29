using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;

namespace PortalLioConnecta.Api.Features.Communications.Queries.GetCommunicationBySlug;

public record GetCommunicationBySlugQuery(string Slug, Guid? PortalUserId = null) : IRequest<CommunicationDto?>;
