using MediatR;
using PortalLioConnecta.Api.Contracts.Admin.TotvsRm;
using PortalLioConnecta.Api.Interfaces;

namespace PortalLioConnecta.Api.Features.Admin.TotvsRm.GetConfiguration;

public class GetAdminTotvsRmConfigurationQueryHandler : IRequestHandler<GetAdminTotvsRmConfigurationQuery, TotvsRmConfigurationDto>
{
    private readonly ITotvsRmConfigurationService _totvsRmConfigurationService;

    public GetAdminTotvsRmConfigurationQueryHandler(ITotvsRmConfigurationService totvsRmConfigurationService)
    {
        _totvsRmConfigurationService = totvsRmConfigurationService;
    }

    public Task<TotvsRmConfigurationDto> Handle(GetAdminTotvsRmConfigurationQuery request, CancellationToken cancellationToken)
    {
        return _totvsRmConfigurationService.GetAsync(cancellationToken);
    }
}
