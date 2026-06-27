using MediatR;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalRH.Api.Features.Admin.MicrosoftGraph.TestConnection;

public sealed record TestAdminMicrosoftGraphConnectionCommand(UpsertMicrosoftGraphConfigurationRequest Request)
    : IRequest<MicrosoftGraphConnectionTestResponse>;
