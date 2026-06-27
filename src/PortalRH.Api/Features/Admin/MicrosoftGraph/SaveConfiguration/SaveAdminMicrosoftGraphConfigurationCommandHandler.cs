using MediatR;
using PortalRH.Api.Contracts.Admin.MicrosoftGraph;
using PortalRH.Api.Interfaces;

namespace PortalRH.Api.Features.Admin.MicrosoftGraph.SaveConfiguration;

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
