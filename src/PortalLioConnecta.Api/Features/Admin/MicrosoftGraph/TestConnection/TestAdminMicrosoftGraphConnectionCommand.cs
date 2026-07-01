using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.TestConnection;

public sealed record TestAdminMicrosoftGraphConnectionCommand(UpsertMicrosoftGraphConfigurationRequest Request)
    : IRequest<MicrosoftGraphConnectionTestResponse>;
