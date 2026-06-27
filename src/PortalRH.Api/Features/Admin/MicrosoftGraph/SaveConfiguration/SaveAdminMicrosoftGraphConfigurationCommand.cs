using MediatR;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;

namespace PortalRH.Api.Features.Admin.MicrosoftGraph.SaveConfiguration;

public sealed record SaveAdminMicrosoftGraphConfigurationCommand(UpsertMicrosoftGraphConfigurationRequest Request) : IRequest<MicrosoftGraphConfigurationDto>;
