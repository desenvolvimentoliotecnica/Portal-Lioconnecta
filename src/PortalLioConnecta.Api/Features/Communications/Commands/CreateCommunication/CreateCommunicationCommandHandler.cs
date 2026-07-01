using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Communications.Commands.CreateCommunication;

public class CreateCommunicationCommandHandler : IRequestHandler<CreateCommunicationCommand, CommunicationDto>
{
    private readonly ICommunicationService _communicationService;

    public CreateCommunicationCommandHandler(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    public Task<CommunicationDto> Handle(CreateCommunicationCommand request, CancellationToken cancellationToken)
    {
        return _communicationService.CreateAsync(request.Request, cancellationToken);
    }
}
