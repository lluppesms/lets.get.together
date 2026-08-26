//-----------------------------------------------------------------------
// <copyright file="Event.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Event Table
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Data.Models;

/// <summary>
/// Represents an event scheduled within a circle.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("Event", Schema = "Meetings")]
public class Event
{
    /// <summary>
    /// Gets or sets the unique identifier for the event.
    /// </summary>
    [Key, Column(Order = 0)]
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the associated circle identifier.
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the title of the event.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional event details.
    /// </summary>
    [StringLength(2000)]
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the UTC start date and time for the event.
    /// </summary>
    public DateTime StartsUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC end date and time for the event.
    /// </summary>
    public DateTime? EndsUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event repeats.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the RSVP mode for a recurring event.
    /// </summary>
    public RsvpMode RsvpMode { get; set; } = RsvpMode.PerOccurrence;

    /// <summary>
    /// Gets or sets the recurrence rule for a recurring event.
    /// </summary>
    [StringLength(200)]
    public string? RecurrenceRule { get; set; }

    /// <summary>
    /// Gets or sets the user who created this event.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the event was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC date and time when the event was cancelled.
    /// </summary>
    public DateTime? CancelledUtc { get; set; }

    /// <summary>
    /// Gets the circle this event belongs to.
    /// </summary>
    public Circle? Circle { get; set; }

    /// <summary>
    /// Gets the user who created this event.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Gets RSVPs for this event.
    /// </summary>
    public ICollection<RSVP> Rsvps { get; set; } = [];

    /// <summary>
    /// Gets reminder logs for this event.
    /// </summary>
    public ICollection<ReminderLog> ReminderLogs { get; set; } = [];
}
