using MediatR;
using PortalLioConnecta.Api.Contracts.Communications;

namespace PortalLioConnecta.Api.Features.Communications.Commands.CreateCommunication;

public record CreateCommunicationCommand(UpsertCommunicationRequest Request) : IRequest<CommunicationDto>;
