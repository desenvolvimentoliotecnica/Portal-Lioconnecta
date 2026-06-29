using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.MicrosoftGraph;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.MicrosoftGraph.SaveConfiguration;

public class SaveAdminMicrosoftGraphConfigurationCommandHandler : IRequestHandler<SaveAdminMicrosoftGraphConfigurationCommand, MicrosoftGraphConfigurationDto>
{
    private readonly IMicrosoftGraphConfigurationService _microsoftGraphConfigurationService;

    public SaveAdminMicrosoftGraphConfigurationCommandHandler(IMicrosoftGraphConfigurationService microsoftGraphConfigurationService)
    {
        _microsoftGraphConfigurationService = microsoftGraphConfigurationService;
    }

    public Task<MicrosoftGraphConfigurationDto> Handle(SaveAdminMicrosoftGraphConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _microsoftGraphConfigurationService.SaveAsync(request.Request, cancellationToken);
    }
}
