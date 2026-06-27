using MediatR;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalRH.Api.Features.Admin.MicrosoftGraph.GetConfiguration;

public sealed record GetAdminMicrosoftGraphConfigurationQuery : IRequest<MicrosoftGraphConfigurationDto>;
