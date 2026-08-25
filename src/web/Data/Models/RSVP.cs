//-----------------------------------------------------------------------
// <copyright file="RSVP.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// RSVP Table
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents an RSVP from a user for an event.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("RSVP", Schema = "Dad")]
public class RSVP
{
    /// <summary>
    /// Gets or sets the unique identifier for the RSVP.
    /// </summary>
    [Key, Column(Order = 0)]
    public int RsvpId { get; set; }

    /// <summary>
    /// Gets or sets the associated event identifier.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the associated circle identifier.
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the associated user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the RSVP status.
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets optional RSVP notes.
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when this RSVP was recorded.
    /// </summary>
    public DateTime RespondedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the associated event.
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// Gets the associated user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets the associated circle membership.
    /// </summary>
    public CircleMembership? CircleMembership { get; set; }
}
