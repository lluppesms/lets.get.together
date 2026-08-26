//-----------------------------------------------------------------------
// <copyright file="EventOccurrence.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Event Occurrence Instance
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Services;

/// <summary>
/// Represents a specific occurrence instance of an event (one-off or expanded from a recurrence rule).
/// </summary>
public class EventOccurrence
{
    /// <summary>
    /// Gets or sets the source event.
    /// </summary>
    public Event Event { get; set; } = null!;

    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public int EventId => Event?.EventId ?? 0;

    /// <summary>
    /// Gets the circle identifier.
    /// </summary>
    public int CircleId => Event?.CircleId ?? 0;

    /// <summary>
    /// Gets the event title.
    /// </summary>
    public string Title => Event?.Title ?? string.Empty;

    /// <summary>
    /// Gets or sets the calculated UTC start time for this occurrence.
    /// </summary>
    public DateTime StartsUtc { get; set; }

    /// <summary>
    /// Gets or sets the calculated UTC end time for this occurrence.
    /// </summary>
    public DateTime? EndsUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is an occurrence of a recurring series.
    /// </summary>
    public bool IsOccurrence { get; set; }
}
