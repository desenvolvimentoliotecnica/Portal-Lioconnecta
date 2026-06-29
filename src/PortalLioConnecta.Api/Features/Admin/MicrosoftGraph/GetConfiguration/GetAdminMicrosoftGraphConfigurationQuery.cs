using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.GetConfiguration;

public sealed record GetAdminMicrosoftGraphConfigurationQuery : IRequest<MicrosoftGraphConfigurationDto>;
