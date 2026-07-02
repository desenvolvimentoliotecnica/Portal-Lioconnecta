using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.TestConnection;

public sealed record TestAdminTotvsRmConnectionCommand(UpsertTotvsRmConfigurationRequest Request)
    : IRequest<TotvsRmConnectionTestResponse>;
