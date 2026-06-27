using MediatR;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Interfaces;

namespace PortalRH.Api.Features.Admin.MicrosoftGraph.GetConfiguration;

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
