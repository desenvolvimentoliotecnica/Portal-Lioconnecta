using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.SaveConfiguration;

public sealed record SaveAdminMicrosoftGraphConfigurationCommand(UpsertMicrosoftGraphConfigurationRequest Request) : IRequest<MicrosoftGraphConfigurationDto>;
