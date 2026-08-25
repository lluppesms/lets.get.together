//-----------------------------------------------------------------------
// <copyright file="CircleMembership.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// CircleMembership Table
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents membership of a user in a circle.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("CircleMembership", Schema = "Dad")]
public class CircleMembership
{
    /// <summary>
    /// Gets or sets the unique identifier for the membership record.
    /// </summary>
    [Key, Column(Order = 0)]
    public int CircleMembershipId { get; set; }

    /// <summary>
    /// Gets or sets the associated circle identifier.
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the associated user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the role of the member within the circle.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Member";

    /// <summary>
    /// Gets or sets the UTC date and time when the user joined the circle.
    /// </summary>
    public DateTime JoinedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC date and time when the user left the circle.
    /// </summary>
    public DateTime? LeftUtc { get; set; }

    /// <summary>
    /// Gets the circle for this membership.
    /// </summary>
    public Circle? Circle { get; set; }

    /// <summary>
    /// Gets the user for this membership.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets RSVPs made through this membership.
    /// </summary>
    public ICollection<RSVP> Rsvps { get; set; } = [];
}
