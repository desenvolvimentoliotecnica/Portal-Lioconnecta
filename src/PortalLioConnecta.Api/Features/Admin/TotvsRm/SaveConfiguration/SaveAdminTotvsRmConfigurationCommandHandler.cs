using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.SaveConfiguration;

public class SaveAdminTotvsRmConfigurationCommandHandler : IRequestHandler<SaveAdminTotvsRmConfigurationCommand, TotvsRmConfigurationDto>
{
    private readonly ITotvsRmConfigurationService _totvsRmConfigurationService;

    public SaveAdminTotvsRmConfigurationCommandHandler(ITotvsRmConfigurationService totvsRmConfigurationService)
    {
        _totvsRmConfigurationService = totvsRmConfigurationService;
    }

    public Task<TotvsRmConfigurationDto> Handle(SaveAdminTotvsRmConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _totvsRmConfigurationService.SaveAsync(request.Request, cancellationToken);
    }
}
