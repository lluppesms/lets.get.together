using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GetTogether.Data.Models;

/// <summary>
/// Represents a single-use verification token for an invitation or email alias.
/// </summary>
[Table("EmailVerificationToken", Schema = "Meetings")]
public class EmailVerificationToken
{
    /// <summary>
    /// Gets or sets the verification token identifier.
    /// </summary>
    [Key]
    public int EmailVerificationTokenId { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 hash of the token delivered to the mailbox.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized mailbox address for which the token was issued.
    /// </summary>
    [Required]
    [StringLength(320)]
    public string NormalizedEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional invitation being verified.
    /// </summary>
    public int? InvitationCodeId { get; set; }

    /// <summary>
    /// Gets or sets the optional email alias being verified.
    /// </summary>
    public int? UserEmailAliasId { get; set; }

    /// <summary>
    /// Gets or sets the expiration time for this token.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets the time at which this token was consumed.
    /// </summary>
    public DateTime? UsedUtc { get; set; }

    /// <summary>
    /// Gets or sets the token creation time.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}