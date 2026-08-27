using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GetTogether.Data.Models;

/// <summary>
/// Represents an external provider identity linked to an application user.
/// </summary>
[Table("UserIdentity", Schema = "Meetings")]
public class UserIdentity
{
    /// <summary>
    /// Gets or sets the unique identifier for this identity record.
    /// </summary>
    [Key]
    public int UserIdentityId { get; set; }

    /// <summary>
    /// Gets or sets the owning application user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the external authentication provider.
    /// </summary>
    public ExternalIdentityProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the provider issuer that issued the subject.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider subject identifier.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC time when this identity was linked.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the owning application user.
    /// </summary>
    public User? User { get; set; }
}