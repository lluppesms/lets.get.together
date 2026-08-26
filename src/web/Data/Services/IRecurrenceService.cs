//-----------------------------------------------------------------------
// <copyright file="IRecurrenceService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Recurrence Service Interface
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Services;

/// <summary>
/// Service interface for expanding recurring events into individual occurrence instances.
/// </summary>
public interface IRecurrenceService
{
    /// <summary>
    /// Expands recurring (and one-off) events into occurrence instances within a date window.
    /// </summary>
    /// <param name="events">The list of events to expand.</param>
    /// <param name="windowStart">The UTC window start date.</param>
    /// <param name="windowEnd">The UTC window end date.</param>
    /// <returns>A collection of occurrence instances ordered by start time.</returns>
    IEnumerable<EventOccurrence> ExpandEvents(IEnumerable<Event> events, DateTime windowStart, DateTime windowEnd);

    /// <summary>
    /// Expands a single event into occurrence instances within a date window.
    /// </summary>
    /// <param name="ev">The event to expand.</param>
    /// <param name="windowStart">The UTC window start date.</param>
    /// <param name="windowEnd">The UTC window end date.</param>
    /// <returns>A collection of occurrence instances for this event within the date window.</returns>
    IEnumerable<EventOccurrence> ExpandEvent(Event ev, DateTime windowStart, DateTime windowEnd);
}
