using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Communications;

public class UpsertCommunicationRequest
{
    [Required]
    [MaxLength(80)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Priority { get; set; } = string.Empty;

    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Channel { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string AttachmentLabel { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Owner { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime PublishedAt { get; set; }
}
