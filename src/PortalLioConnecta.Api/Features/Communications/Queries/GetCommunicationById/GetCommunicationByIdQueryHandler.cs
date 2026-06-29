using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Communications.Queries.GetCommunicationById;

public class GetCommunicationByIdQueryHandler : IRequestHandler<GetCommunicationByIdQuery, CommunicationDto?>
{
    private readonly ICommunicationService _communicationService;

    public GetCommunicationByIdQueryHandler(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    public Task<CommunicationDto?> Handle(GetCommunicationByIdQuery request, CancellationToken cancellationToken)
    {
        return _communicationService.GetByIdAsync(request.Id, request.PortalUserId, cancellationToken);
    }
}
