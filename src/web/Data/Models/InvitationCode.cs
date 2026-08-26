//-----------------------------------------------------------------------
// <copyright file="InvitationCode.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// InvitationCode Table
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Data.Models;

/// <summary>
/// Represents an invitation code used to join a circle.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("InvitationCode", Schema = "Meetings")]
public class InvitationCode
{
    /// <summary>
    /// Gets or sets the unique identifier for the invitation code record.
    /// </summary>
    [Key, Column(Order = 0)]
    public int InvitationCodeId { get; set; }

    /// <summary>
    /// Gets or sets the associated circle identifier.
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the invitation code string.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who created the invitation code.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the code was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC date and time when the code expires.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets the user who redeemed the invitation code.
    /// </summary>
    public int? RedeemedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the code was redeemed.
    /// </summary>
    public DateTime? RedeemedUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the code was revoked.
    /// </summary>
    public DateTime? RevokedUtc { get; set; }

    /// <summary>
    /// Gets the circle associated with this invitation code.
    /// </summary>
    public Circle? Circle { get; set; }

    /// <summary>
    /// Gets the user who created this invitation code.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Gets the user who redeemed this invitation code.
    /// </summary>
    public User? RedeemedByUser { get; set; }
}
