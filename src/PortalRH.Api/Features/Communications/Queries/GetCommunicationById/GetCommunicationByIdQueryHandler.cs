using MediatR;
using PortalRH.Api.Contracts.Communications;
using PortalRH.Api.Interfaces;

namespace PortalRH.Api.Features.Communications.Queries.GetCommunicationById;

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
