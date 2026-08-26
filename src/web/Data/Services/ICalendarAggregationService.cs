//-----------------------------------------------------------------------
// <copyright file="ICalendarAggregationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Calendar Aggregation Service Interface
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Services;

namespace GetTogether.Data.Services;

/// <summary>
/// Service interface for aggregating calendar events across circles for a user.
/// </summary>
public interface ICalendarAggregationService
{
    /// <summary>
    /// Fetches all calendar events and expanded occurrences for a user across all active circles within a date window.
    /// </summary>
    /// <param name="requestingUserId">The identifier of the user requesting the calendar.</param>
    /// <param name="startUtc">The UTC start of the date window.</param>
    /// <param name="endUtc">The UTC end of the date window.</param>
    /// <returns>A collection of aggregated calendar event items ordered by start time.</returns>
    Task<IList<CalendarEventItem>> GetCalendarEventsForUserAsync(int requestingUserId, DateTime startUtc, DateTime endUtc);
}
