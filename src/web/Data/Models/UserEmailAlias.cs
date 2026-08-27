using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GetTogether.Data.Models;

/// <summary>
/// Represents an email alias controlled and optionally verified by an application user.
/// </summary>
[Table("UserEmailAlias", Schema = "Meetings")]
public class UserEmailAlias
{
    /// <summary>
    /// Gets or sets the unique identifier for this email alias record.
    /// </summary>
    [Key]
    public int UserEmailAliasId { get; set; }

    /// <summary>
    /// Gets or sets the owning application user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address supplied by the user.
    /// </summary>
    [Required]
    [StringLength(320)]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized email address used for uniqueness checks.
    /// </summary>
    [Required]
    [StringLength(320)]
    public string NormalizedEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether mailbox control has been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the user's designated email alias.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets the UTC time at which the email alias was verified.
    /// </summary>
    public DateTime? VerifiedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC time when this email alias was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the owning application user.
    /// </summary>
    public User? User { get; set; }
}