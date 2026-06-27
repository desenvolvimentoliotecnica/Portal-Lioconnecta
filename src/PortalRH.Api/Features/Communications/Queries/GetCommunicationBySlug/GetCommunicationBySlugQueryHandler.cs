using MediatR;
using PortalRH.Api.Contracts.Communications;
using PortalRH.Api.Interfaces;

namespace PortalRH.Api.Features.Communications.Queries.GetCommunicationBySlug;

public class GetCommunicationBySlugQueryHandler : IRequestHandler<GetCommunicationBySlugQuery, CommunicationDto?>
{
    private readonly ICommunicationService _communicationService;

    public GetCommunicationBySlugQueryHandler(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    public Task<CommunicationDto?> Handle(GetCommunicationBySlugQuery request, CancellationToken cancellationToken)
    {
        return _communicationService.GetBySlugAsync(request.Slug, request.PortalUserId, cancellationToken);
    }
}
