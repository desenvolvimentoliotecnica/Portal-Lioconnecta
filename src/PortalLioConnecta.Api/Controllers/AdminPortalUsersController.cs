using Microsoft.AspNetCore.Mvc;
using PortalLioConnecta.Api.Contracts.Admin.PortalUsers;
using PortalLioConnecta.Api.Interfaces;
using PortalLioConnecta.Api.Security;

namespace PortalLioConnecta.Api.Controllers;

[ApiController]
[Route("api/admin/portal-users")]
[RequireSuperAdminSession]
public class AdminPortalUsersController : ControllerBase
{
    private readonly IPortalUserAdminService _portalUserAdminService;

    public AdminPortalUsersController(IPortalUserAdminService portalUserAdminService)
    {
        _portalUserAdminService = portalUserAdminService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PortalUserAdminListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? query,
        [FromQuery] string? status,
        [FromQuery] string? role,
        [FromQuery] string? department,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 8,
        CancellationToken cancellationToken = default)
    {
        var items = await _portalUserAdminService.GetAllAsync(query, status, role, department, sortBy, sortDirection, page, pageSize, cancellationToken);
        return Ok(items);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(PortalUserAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePortalUserStatusRequest request, CancellationToken cancellationToken)
    {
        var actor = AdminSessionHttpContext.Get(HttpContext)?.User;
        if (actor is null)
        {
            return Unauthorized();
        }

        var item = await _portalUserAdminService.UpdateStatusAsync(id, request.IsActive, actor, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(typeof(PortalUserAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdatePortalUserRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = AdminSessionHttpContext.Get(HttpContext)?.User;
        if (actor is null)
        {
            return Unauthorized();
        }

        var item = await _portalUserAdminService.UpdateRoleAsync(id, request.Role, actor, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPatch("{id:guid}/permissions")]
    [ProducesResponseType(typeof(PortalUserAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdatePortalUserModulePermissionRequest request, CancellationToken cancellationToken)
    {
        var actor = AdminSessionHttpContext.Get(HttpContext)?.User;
        if (actor is null)
        {
            return Unauthorized();
        }

        var item = await _portalUserAdminService.UpdateModulePermissionAsync(id, request.ModuleKey, request.AccessLevel, actor, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}
