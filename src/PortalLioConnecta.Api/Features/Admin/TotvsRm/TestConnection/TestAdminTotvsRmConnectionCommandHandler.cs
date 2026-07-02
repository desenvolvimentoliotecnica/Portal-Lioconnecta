using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.TestConnection;

public class TestAdminTotvsRmConnectionCommandHandler
    : IRequestHandler<TestAdminTotvsRmConnectionCommand, TotvsRmConnectionTestResponse>
{
    private readonly ITotvsRmConfigurationService _totvsRmConfigurationService;

    public TestAdminTotvsRmConnectionCommandHandler(ITotvsRmConfigurationService totvsRmConfigurationService)
    {
        _totvsRmConfigurationService = totvsRmConfigurationService;
    }

    public Task<TotvsRmConnectionTestResponse> Handle(
        TestAdminTotvsRmConnectionCommand request,
        CancellationToken cancellationToken)
    {
        return _totvsRmConfigurationService.TestConnectionAsync(request.Request, cancellationToken);
    }
}
