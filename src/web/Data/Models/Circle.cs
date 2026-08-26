//-----------------------------------------------------------------------
// <copyright file="Circle.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Circle Table
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Data.Models;

/// <summary>
/// Represents a social circle in the Get Together domain.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("Circle", Schema = "Dad")]
public class Circle
{
    /// <summary>
    /// Gets or sets the unique identifier for the circle.
    /// </summary>
    [Key, Column(Order = 0)]
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the circle.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the circle.
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the user who created the circle.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the circle was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the circle has been archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets the user who created the circle.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Gets memberships for this circle.
    /// </summary>
    public ICollection<CircleMembership> Members { get; set; } = [];

    /// <summary>
    /// Gets invitation codes associated with this circle.
    /// </summary>
    public ICollection<InvitationCode> InvitationCodes { get; set; } = [];

    /// <summary>
    /// Gets events for this circle.
    /// </summary>
    public ICollection<Event> Events { get; set; } = [];
}
