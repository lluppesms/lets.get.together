//-----------------------------------------------------------------------
// <copyright file="CalendarAggregationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Calendar Aggregation Service Implementation
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;

namespace GetTogether.Data.Services;

/// <summary>
/// Service for aggregating and expanding calendar events across all active circles for a user.
/// </summary>
public class CalendarAggregationService : ICalendarAggregationService
{
    private static readonly string[] CirclePalette =
    [
        "#4F46E5", // Indigo
        "#059669", // Emerald
        "#D97706", // Amber
        "#DC2626", // Red
        "#7C3AED", // Violet
        "#2563EB", // Blue
        "#DB2777", // Pink
        "#0891B2"  // Cyan
    ];

    private readonly ICircleRepository _circleRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IRsvpRepository _rsvpRepository;
    private readonly IRecurrenceService _recurrenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarAggregationService"/> class.
    /// </summary>
    public CalendarAggregationService(
        ICircleRepository circleRepository,
        IEventRepository eventRepository,
        IRsvpRepository rsvpRepository,
        IRecurrenceService recurrenceService)
    {
        _circleRepository = circleRepository ?? throw new ArgumentNullException(nameof(circleRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _rsvpRepository = rsvpRepository ?? throw new ArgumentNullException(nameof(rsvpRepository));
        _recurrenceService = recurrenceService ?? throw new ArgumentNullException(nameof(recurrenceService));
    }

    /// <inheritdoc/>
    public async Task<IList<CalendarEventItem>> GetCalendarEventsForUserAsync(int requestingUserId, DateTime startUtc, DateTime endUtc)
    {
        if (startUtc > endUtc)
        {
            throw new ArgumentException("startUtc must be less than or equal to endUtc.", nameof(startUtc));
        }

        var activeCircles = await _circleRepository.GetCirclesForUserAsync(requestingUserId);
        if (activeCircles.Count == 0)
        {
            return [];
        }

        var result = new List<CalendarEventItem>();

        for (var i = 0; i < activeCircles.Count; i++)
        {
            var circle = activeCircles[i];
            var colorIndex = i % CirclePalette.Length;
            var circleColor = CirclePalette[colorIndex];

            var events = await _eventRepository.GetEventsByCircleAsync(circle.CircleId, requestingUserId);
            if (events.Count == 0)
            {
                continue;
            }

            var expandedOccurrences = _recurrenceService.ExpandEvents(events, startUtc, endUtc);
            var rsvpCache = new Dictionary<int, IList<RSVP>>();

            foreach (var occ in expandedOccurrences)
            {
                var ev = occ.Event;
                if (!rsvpCache.TryGetValue(ev.EventId, out var rsvps))
                {
                    rsvps = await _rsvpRepository.GetRsvpsByEventAsync(ev.EventId, requestingUserId);
                    rsvpCache[ev.EventId] = rsvps;
                }

                RSVP? userRsvp = null;
                if (ev.IsRecurring && ev.RsvpMode == RsvpMode.PerOccurrence)
                {
                    userRsvp = rsvps.FirstOrDefault(r => r.UserId == requestingUserId && r.OccurrenceDate.HasValue && r.OccurrenceDate.Value.Date == occ.StartsUtc.Date);
                }

                userRsvp ??= rsvps.FirstOrDefault(r => r.UserId == requestingUserId && (!r.OccurrenceDate.HasValue || !ev.IsRecurring || ev.RsvpMode == RsvpMode.Series));

                result.Add(new CalendarEventItem
                {
                    EventId = ev.EventId,
                    CircleId = circle.CircleId,
                    CircleName = circle.Name,
                    CircleColorIndex = colorIndex,
                    CircleColor = circleColor,
                    Title = ev.Title,
                    Details = ev.Details,
                    StartsUtc = occ.StartsUtc,
                    EndsUtc = occ.EndsUtc,
                    IsRecurring = ev.IsRecurring,
                    IsOccurrence = occ.IsOccurrence,
                    RsvpMode = ev.RsvpMode,
                    RecurrenceRule = ev.RecurrenceRule,
                    UserRsvpStatus = userRsvp?.Status ?? "Unanswered",
                    UserRsvpNotes = userRsvp?.Notes,
                    Event = ev
                });
            }
        }

        return result.OrderBy(c => c.StartsUtc).ThenBy(c => c.Title).ToList();
    }
}
