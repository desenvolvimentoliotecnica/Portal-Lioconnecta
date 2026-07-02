using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.GetConfiguration;

public sealed record GetAdminTotvsRmConfigurationQuery : IRequest<TotvsRmConfigurationDto>;
