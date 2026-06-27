using PortalRH.Api.Contracts.HrProfile;
using PortalRH.Api.Interfaces;
using PortalRH.Api.Models;

namespace PortalRH.Api.Services;

public class HrProfileService : IHrProfileService
{
    public Task<HrProfileResponse> GetProfileAsync(PortalUser user, CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var response = new HrProfileResponse(
            user.DisplayName,
            user.Department ?? string.Empty,
            user.Title ?? string.Empty,
            user.ManagerDisplayName ?? string.Empty,
            [
                new HrProfileItemDto("Ferias (Consultar/Solicitar)", "#perfil-rh/ferias", "TOTVS RM", false),
                new HrProfileItemDto("Holerite", "#perfil-rh/holerite", "TOTVS RM", false),
                new HrProfileItemDto("Beneficios (VR/VT)", "#perfil-rh/beneficios", "TOTVS RM", false),
                new HrProfileItemDto("Minha Avaliacao", "#perfil-rh/avaliacao", "TOTVS RM", false),
                new HrProfileItemDto("Dados Cadastrais", "#perfil-rh/cadastro", "TOTVS RM", false),
                new HrProfileItemDto("Ponto", "#perfil-rh/ponto", "TOTVS RM", false),
                new HrProfileItemDto("Treinamentos", "https://portal.example.local/lms", "LMS", true),
                new HrProfileItemDto("Chamados RH", "https://portal.example.local/servicenow/rh", "ServiceNow", true)
            ],
            "TOTVS RM + ServiceNow + LMS",
            true);

        return Task.FromResult(response);
    }
}
