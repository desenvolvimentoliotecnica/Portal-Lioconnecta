using MediatR;
using PortalRH.Api.Contracts.Communications;

namespace PortalRH.Api.Features.Communications.Queries.GetCommunicationBySlug;

public record GetCommunicationBySlugQuery(string Slug, Guid? PortalUserId = null) : IRequest<CommunicationDto?>;
