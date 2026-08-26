//-----------------------------------------------------------------------
// <copyright file="RecurrenceService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Recurrence Service Implementation
// </summary>
//-----------------------------------------------------------------------
using System.Text.RegularExpressions;
using DadABase.Data.Models;

namespace DadABase.Data.Services;

/// <summary>
/// Service for expanding recurring events (Weekly, Biweekly, Monthly) into occurrence instances.
/// </summary>
public class RecurrenceService : IRecurrenceService
{
    /// <inheritdoc/>
    public IEnumerable<EventOccurrence> ExpandEvents(IEnumerable<Event> events, DateTime windowStart, DateTime windowEnd)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        if (windowStart > windowEnd)
        {
            throw new ArgumentException("windowStart must be less than or equal to windowEnd.", nameof(windowStart));
        }

        var results = new List<EventOccurrence>();
        foreach (var ev in events)
        {
            results.AddRange(ExpandEvent(ev, windowStart, windowEnd));
        }

        return results.OrderBy(o => o.StartsUtc);
    }

    /// <inheritdoc/>
    public IEnumerable<EventOccurrence> ExpandEvent(Event ev, DateTime windowStart, DateTime windowEnd)
    {
        if (ev == null)
        {
            throw new ArgumentNullException(nameof(ev));
        }

        if (windowStart > windowEnd)
        {
            throw new ArgumentException("windowStart must be less than or equal to windowEnd.", nameof(windowStart));
        }

        // Cancelled events generate no occurrences
        if (ev.CancelledUtc.HasValue)
        {
            yield break;
        }

        // Non-recurring events yield a single occurrence if within date window
        if (!ev.IsRecurring || string.IsNullOrWhiteSpace(ev.RecurrenceRule))
        {
            if (ev.StartsUtc >= windowStart && ev.StartsUtc <= windowEnd)
            {
                yield return new EventOccurrence
                {
                    Event = ev,
                    StartsUtc = ev.StartsUtc,
                    EndsUtc = ev.EndsUtc,
                    IsOccurrence = false
                };
            }

            yield break;
        }

        // Recurring event expansion
        var duration = ev.EndsUtc.HasValue && ev.EndsUtc.Value >= ev.StartsUtc
            ? ev.EndsUtc.Value - ev.StartsUtc
            : (TimeSpan?)null;

        var pattern = ParsePattern(ev.RecurrenceRule);

        switch (pattern)
        {
            case RecurrencePattern.Weekly:
                foreach (var occ in ExpandWeekly(ev, windowStart, windowEnd, duration))
                {
                    yield return occ;
                }
                break;

            case RecurrencePattern.Biweekly:
                foreach (var occ in ExpandBiweekly(ev, windowStart, windowEnd, duration))
                {
                    yield return occ;
                }
                break;

            case RecurrencePattern.Monthly:
                foreach (var occ in ExpandMonthly(ev, windowStart, windowEnd, duration))
                {
                    yield return occ;
                }
                break;
        }
    }

    private static IEnumerable<EventOccurrence> ExpandWeekly(Event ev, DateTime windowStart, DateTime windowEnd, TimeSpan? duration)
    {
        var targetDayOfWeek = ParseDayOfWeek(ev.RecurrenceRule) ?? ev.StartsUtc.DayOfWeek;
        var eventTime = ev.StartsUtc.TimeOfDay;

        var currentDate = ev.StartsUtc.Date;
        while (currentDate.DayOfWeek != targetDayOfWeek)
        {
            currentDate = currentDate.AddDays(1);
        }

        while (true)
        {
            var occurrenceStart = DateTime.SpecifyKind(currentDate.Date + eventTime, DateTimeKind.Utc);
            if (occurrenceStart > windowEnd)
            {
                break;
            }

            if (occurrenceStart >= ev.StartsUtc && occurrenceStart >= windowStart)
            {
                yield return new EventOccurrence
                {
                    Event = ev,
                    StartsUtc = occurrenceStart,
                    EndsUtc = duration.HasValue ? occurrenceStart + duration.Value : null,
                    IsOccurrence = true
                };
            }

            currentDate = currentDate.AddDays(7);
        }
    }

    private static IEnumerable<EventOccurrence> ExpandBiweekly(Event ev, DateTime windowStart, DateTime windowEnd, TimeSpan? duration)
    {
        var targetDayOfWeek = ParseDayOfWeek(ev.RecurrenceRule) ?? ev.StartsUtc.DayOfWeek;
        var eventTime = ev.StartsUtc.TimeOfDay;

        var currentDate = ev.StartsUtc.Date;
        while (currentDate.DayOfWeek != targetDayOfWeek)
        {
            currentDate = currentDate.AddDays(1);
        }

        while (true)
        {
            var occurrenceStart = DateTime.SpecifyKind(currentDate.Date + eventTime, DateTimeKind.Utc);
            if (occurrenceStart > windowEnd)
            {
                break;
            }

            if (occurrenceStart >= ev.StartsUtc && occurrenceStart >= windowStart)
            {
                yield return new EventOccurrence
                {
                    Event = ev,
                    StartsUtc = occurrenceStart,
                    EndsUtc = duration.HasValue ? occurrenceStart + duration.Value : null,
                    IsOccurrence = true
                };
            }

            currentDate = currentDate.AddDays(14);
        }
    }

    private static IEnumerable<EventOccurrence> ExpandMonthly(Event ev, DateTime windowStart, DateTime windowEnd, TimeSpan? duration)
    {
        var targetDayOfMonth = ParseDayOfMonth(ev.RecurrenceRule) ?? ev.StartsUtc.Day;
        var eventTime = ev.StartsUtc.TimeOfDay;

        var year = ev.StartsUtc.Year;
        var month = ev.StartsUtc.Month;

        while (true)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var actualDay = Math.Min(targetDayOfMonth, daysInMonth);
            var occurrenceStart = new DateTime(year, month, actualDay, eventTime.Hours, eventTime.Minutes, eventTime.Seconds, DateTimeKind.Utc);

            if (occurrenceStart > windowEnd)
            {
                break;
            }

            if (occurrenceStart >= ev.StartsUtc && occurrenceStart >= windowStart)
            {
                yield return new EventOccurrence
                {
                    Event = ev,
                    StartsUtc = occurrenceStart,
                    EndsUtc = duration.HasValue ? occurrenceStart + duration.Value : null,
                    IsOccurrence = true
                };
            }

            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
        }
    }

    private static RecurrencePattern ParsePattern(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return RecurrencePattern.Weekly;
        }

        var normalized = rule.ToLowerInvariant();
        if (normalized.Contains("biweekly") || normalized.Contains("bi-weekly") || normalized.Contains("every_2_weeks") || normalized.Contains("freq=biweekly"))
        {
            return RecurrencePattern.Biweekly;
        }

        if (normalized.Contains("monthly") || normalized.Contains("freq=monthly"))
        {
            return RecurrencePattern.Monthly;
        }

        return RecurrencePattern.Weekly;
    }

    private static DayOfWeek? ParseDayOfWeek(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return null;
        }

        var match = Regex.Match(rule, @"(?:BYDAY|DAYOFWEEK|DAY)\s*[:=]\s*([A-Za-z0-9]+)", RegexOptions.IgnoreCase);
        var val = match.Success ? match.Groups[1].Value : rule;

        if (Enum.TryParse<DayOfWeek>(val, true, out var dow))
        {
            return dow;
        }

        var words = Regex.Split(rule, @"[;:=,\s]+");
        foreach (var word in words)
        {
            if (Enum.TryParse<DayOfWeek>(word, true, out var parsedDow))
            {
                return parsedDow;
            }

            var abbrev = word.ToUpperInvariant();
            switch (abbrev)
            {
                case "SU": case "SUN": return DayOfWeek.Sunday;
                case "MO": case "MON": return DayOfWeek.Monday;
                case "TU": case "TUE": return DayOfWeek.Tuesday;
                case "WE": case "WED": return DayOfWeek.Wednesday;
                case "TH": case "THU": return DayOfWeek.Thursday;
                case "FR": case "FRI": return DayOfWeek.Friday;
                case "SA": case "SAT": return DayOfWeek.Saturday;
            }
        }

        return null;
    }

    private static int? ParseDayOfMonth(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return null;
        }

        var match = Regex.Match(rule, @"(?:BYMONTHDAY|DAYOFMONTH|DAY)\s*[:=]\s*(\d{1,2})", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var dom) && dom is >= 1 and <= 31)
        {
            return dom;
        }

        var numMatch = Regex.Match(rule, @"\b([1-9]|[12]\d|3[01])\b");
        if (numMatch.Success && int.TryParse(numMatch.Value, out var num) && num is >= 1 and <= 31)
        {
            return num;
        }

        return null;
    }

    private enum RecurrencePattern
    {
        Weekly,
        Biweekly,
        Monthly
    }
}
