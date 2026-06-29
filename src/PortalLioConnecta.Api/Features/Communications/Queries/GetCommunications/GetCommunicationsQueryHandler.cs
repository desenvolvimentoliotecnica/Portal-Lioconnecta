using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Communications.Queries.GetCommunications;

public class GetCommunicationsQueryHandler : IRequestHandler<GetCommunicationsQuery, IReadOnlyList<CommunicationDto>>
{
    private readonly ICommunicationService _communicationService;

    public GetCommunicationsQueryHandler(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    public Task<IReadOnlyList<CommunicationDto>> Handle(GetCommunicationsQuery request, CancellationToken cancellationToken)
    {
        return _communicationService.GetAllAsync(request.PortalUserId, cancellationToken);
    }
}
