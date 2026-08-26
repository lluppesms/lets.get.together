//-----------------------------------------------------------------------
// <copyright file="RecurrenceExpander.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Recurrence Expander Helper
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Helpers;

/// <summary>
/// Helper utility for expanding recurring events into occurrence start dates.
/// </summary>
public static class RecurrenceExpander
{
    /// <summary>
    /// Expands an event's recurrence rule into occurrence start dates up to a specified end date or count limit.
    /// </summary>
    /// <param name="event">The event containing start time and recurrence rule.</param>
    /// <param name="untilUtc">The maximum UTC date/time up to which occurrences are generated.</param>
    /// <param name="maxCount">The maximum number of occurrences to generate (default 100).</param>
    /// <returns>A list of UTC start dates for each occurrence, including the initial start date.</returns>
    public static IList<DateTime> ExpandOccurrences(Event @event, DateTime untilUtc, int maxCount = 100)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return ExpandOccurrences(@event.StartsUtc, @event.IsRecurring, @event.RecurrenceRule, untilUtc, maxCount);
    }

    /// <summary>
    /// Expands a recurrence rule into occurrence start dates starting from <paramref name="startsUtc"/>.
    /// </summary>
    /// <param name="startsUtc">The initial start date/time in UTC.</param>
    /// <param name="isRecurring">Whether the event is recurring.</param>
    /// <param name="recurrenceRule">The recurrence rule string (e.g. "Weekly", "Biweekly", "Monthly", "FREQ=WEEKLY").</param>
    /// <param name="untilUtc">The maximum UTC date/time up to which occurrences are generated.</param>
    /// <param name="maxCount">The maximum number of occurrences to generate.</param>
    /// <returns>A list of UTC start dates for each occurrence, including the initial start date.</returns>
    public static IList<DateTime> ExpandOccurrences(
        DateTime startsUtc,
        bool isRecurring,
        string? recurrenceRule,
        DateTime untilUtc,
        int maxCount = 100)
    {
        var occurrences = new List<DateTime> { startsUtc };

        if (!isRecurring || string.IsNullOrWhiteSpace(recurrenceRule) || maxCount <= 1 || startsUtc >= untilUtc)
        {
            return occurrences;
        }

        var normalizedRule = recurrenceRule.Trim().ToUpperInvariant();

        // Determine frequency & interval
        int daysInterval = 0;
        int monthsInterval = 0;

        if (normalizedRule.Contains("BIWEEKLY") || normalizedRule.Contains("INTERVAL=2"))
        {
            daysInterval = 14;
        }
        else if (normalizedRule.Contains("WEEKLY") || normalizedRule.Contains("FREQ=WEEKLY"))
        {
            daysInterval = 7;
        }
        else if (normalizedRule.Contains("MONTHLY") || normalizedRule.Contains("FREQ=MONTHLY"))
        {
            monthsInterval = 1;
        }
        else
        {
            // Default fallback if rule isn't recognized
            return occurrences;
        }

        var current = startsUtc;
        while (occurrences.Count < maxCount)
        {
            if (daysInterval > 0)
            {
                current = current.AddDays(daysInterval);
            }
            else if (monthsInterval > 0)
            {
                current = current.AddMonths(monthsInterval);
            }

            if (current > untilUtc)
            {
                break;
            }

            occurrences.Add(current);
        }

        return occurrences;
    }
}
