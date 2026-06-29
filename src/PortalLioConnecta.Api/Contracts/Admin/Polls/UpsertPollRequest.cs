using System.ComponentModel.DataAnnotations;

namespace PortalLioConnecta.Api.Contracts.Admin.Polls;

public class UpsertPollRequest
{
    [Required]
    [MaxLength(240)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(120)]
    public string? AttachmentLabel { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [Required]
    [MaxLength(120)]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = string.Empty;

    public bool AllowMultipleChoices { get; set; }

    [Required]
    [MaxLength(40)]
    public string ResultsVisibility { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? ClosesAtUtc { get; set; }

    [Required]
    [MinLength(2)]
    public List<UpsertPollOptionRequest> Options { get; set; } = [];
}
