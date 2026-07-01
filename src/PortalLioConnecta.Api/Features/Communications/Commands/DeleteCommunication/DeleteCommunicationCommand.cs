using MediatR;

namespace PortalLioConnecta.Api.Features.Communications.Commands.DeleteCommunication;

public record DeleteCommunicationCommand(Guid Id) : IRequest<bool>;
