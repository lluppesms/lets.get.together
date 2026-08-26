//-----------------------------------------------------------------------
// <copyright file="CalendarEventItem.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Aggregated Calendar Event Item DTO
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Services;

/// <summary>
/// Represents an aggregated event or expanded occurrence for calendar display.
/// </summary>
public class CalendarEventItem
{
    /// <summary>
    /// Gets or sets the source event identifier.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the circle identifier.
    /// </summary>
    public int CircleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the circle.
    /// </summary>
    public string CircleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the circle color index.
    /// </summary>
    public int CircleColorIndex { get; set; }

    /// <summary>
    /// Gets or sets the circle display color (hex value).
    /// </summary>
    public string CircleColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional event details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the UTC start date and time for this occurrence/event.
    /// </summary>
    public DateTime StartsUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC end date and time for this occurrence/event.
    /// </summary>
    public DateTime? EndsUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source event is recurring.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is an expanded occurrence of a recurring series.
    /// </summary>
    public bool IsOccurrence { get; set; }

    /// <summary>
    /// Gets or sets the RSVP mode for a recurring event.
    /// </summary>
    public RsvpMode RsvpMode { get; set; } = RsvpMode.PerOccurrence;

    /// <summary>
    /// Gets or sets the recurrence rule for a recurring event.
    /// </summary>
    public string? RecurrenceRule { get; set; }

    /// <summary>
    /// Gets or sets the requesting user's RSVP status (e.g. Accept, Decline, Maybe, Unanswered).
    /// </summary>
    public string UserRsvpStatus { get; set; } = "Unanswered";

    /// <summary>
    /// Gets or sets the requesting user's RSVP notes.
    /// </summary>
    public string? UserRsvpNotes { get; set; }

    /// <summary>
    /// Gets or sets the underlying source Event object.
    /// </summary>
    public Event? Event { get; set; }
}
