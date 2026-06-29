using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Auth;

public sealed class LdapLoginRequest
{
    [Required]
    [MaxLength(200)]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
