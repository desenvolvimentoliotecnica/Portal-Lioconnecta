using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.GetConfiguration;

public class GetAdminMicrosoftGraphConfigurationQueryHandler : IRequestHandler<GetAdminMicrosoftGraphConfigurationQuery, MicrosoftGraphConfigurationDto>
{
    private readonly IMicrosoftGraphConfigurationService _microsoftGraphConfigurationService;

    public GetAdminMicrosoftGraphConfigurationQueryHandler(IMicrosoftGraphConfigurationService microsoftGraphConfigurationService)
    {
        _microsoftGraphConfigurationService = microsoftGraphConfigurationService;
    }

    public Task<MicrosoftGraphConfigurationDto> Handle(GetAdminMicrosoftGraphConfigurationQuery request, CancellationToken cancellationToken)
    {
        return _microsoftGraphConfigurationService.GetAsync(cancellationToken);
    }
}
