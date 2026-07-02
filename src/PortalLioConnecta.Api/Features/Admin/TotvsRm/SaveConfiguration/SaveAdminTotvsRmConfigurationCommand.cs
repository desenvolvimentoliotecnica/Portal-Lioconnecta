using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.SaveConfiguration;

public sealed record SaveAdminTotvsRmConfigurationCommand(UpsertTotvsRmConfigurationRequest Request) : IRequest<TotvsRmConfigurationDto>;
