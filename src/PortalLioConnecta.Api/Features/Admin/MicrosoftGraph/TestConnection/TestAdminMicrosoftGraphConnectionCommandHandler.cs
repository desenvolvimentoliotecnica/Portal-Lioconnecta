using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.TestConnection;

public class TestAdminMicrosoftGraphConnectionCommandHandler
    : IRequestHandler<TestAdminMicrosoftGraphConnectionCommand, MicrosoftGraphConnectionTestResponse>
{
    private readonly IMicrosoftGraphConfigurationService _microsoftGraphConfigurationService;

    public TestAdminMicrosoftGraphConnectionCommandHandler(IMicrosoftGraphConfigurationService microsoftGraphConfigurationService)
    {
        _microsoftGraphConfigurationService = microsoftGraphConfigurationService;
    }

    public Task<MicrosoftGraphConnectionTestResponse> Handle(
        TestAdminMicrosoftGraphConnectionCommand request,
        CancellationToken cancellationToken)
    {
        return _microsoftGraphConfigurationService.TestConnectionAsync(request.Request, cancellationToken);
    }
}
