using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;

namespace PortalLioConnecta.Api.Features.Communications.Commands.UpdateCommunication;

public record UpdateCommunicationCommand(Guid Id, UpsertCommunicationRequest Request) : IRequest<CommunicationDto?>;
