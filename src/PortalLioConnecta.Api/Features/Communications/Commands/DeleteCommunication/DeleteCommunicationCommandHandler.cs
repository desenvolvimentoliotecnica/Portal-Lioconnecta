using MediatR;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Communications.Commands.DeleteCommunication;

public class DeleteCommunicationCommandHandler : IRequestHandler<DeleteCommunicationCommand, bool>
{
    private readonly ICommunicationService _communicationService;

    public DeleteCommunicationCommandHandler(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    public Task<bool> Handle(DeleteCommunicationCommand request, CancellationToken cancellationToken)
    {
        return _communicationService.DeleteAsync(request.Id, cancellationToken);
    }
}
