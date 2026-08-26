//-----------------------------------------------------------------------
// <copyright file="CalendarAggregation_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Calendar Aggregation and Privacy Unit Tests
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Tests;

using GetTogether.Data;
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;
using GetTogether.Data.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class CalendarAggregation_Tests
{
    // =========================================================================
    // 1. Multi-circle calendar event aggregation
    // =========================================================================

    [Fact]
    public async Task GetUpcomingEventsForUser_AggregatesEventsAcrossMultipleCircles()
    {
        await using var context = CreateContext();
        
        // User 10 (Alice) belongs to Circle 1 ("Pickleball Crew") and Circle 2 ("Coffee Club")
        AddUser(context, 10, "Alice");
        AddUser(context, 20, "Bob");

        context.Circles!.AddRange(
            new Circle { CircleId = 1, Name = "Pickleball Crew", CreatedByUserId = 10 },
            new Circle { CircleId = 2, Name = "Coffee Club", CreatedByUserId = 20 });

        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 10 },
            new CircleMembership { CircleId = 2, UserId = 10 },
            new CircleMembership { CircleId = 2, UserId = 20 });

        var now = DateTime.UtcNow;
        var event1 = new Event
        {
            EventId = 101,
            CircleId = 1,
            Title = "Saturday Pickleball",
            StartsUtc = now.AddDays(2),
            CreatedByUserId = 10
        };

        var event2 = new Event
        {
            EventId = 102,
            CircleId = 2,
            Title = "Tuesday Morning Coffee",
            StartsUtc = now.AddDays(1),
            CreatedByUserId = 20
        };

        var event3 = new Event
        {
            EventId = 103,
            CircleId = 1,
            Title = "Next Week Tournament",
            StartsUtc = now.AddDays(5),
            CreatedByUserId = 10
        };

        context.Events!.AddRange(event1, event2, event3);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var aggregatedEvents = await repository.GetUpcomingEventsForUserAsync(10);

        Assert.Equal(3, aggregatedEvents.Count);

        // Verify chronological ordering by start time
        Assert.Equal("Tuesday Morning Coffee", aggregatedEvents[0].Title);
        Assert.Equal(2, aggregatedEvents[0].CircleId);
        Assert.Equal("Coffee Club", aggregatedEvents[0].Circle.Name);

        Assert.Equal("Saturday Pickleball", aggregatedEvents[1].Title);
        Assert.Equal(1, aggregatedEvents[1].CircleId);
        Assert.Equal("Pickleball Crew", aggregatedEvents[1].Circle.Name);

        Assert.Equal("Next Week Tournament", aggregatedEvents[2].Title);
        Assert.Equal(1, aggregatedEvents[2].CircleId);
        Assert.Equal("Pickleball Crew", aggregatedEvents[2].Circle.Name);
    }

    // =========================================================================
    // 2. Circle privacy enforcement for calendar aggregation
    // =========================================================================

    [Fact]
    public async Task GetUpcomingEventsForUser_ExcludesEventsFromUnjoinedAndLeftCircles()
    {
        await using var context = CreateContext();

        AddUser(context, 10, "Alice");
        AddUser(context, 20, "Bob");
        AddUser(context, 30, "Charlie");

        // Circle 1: Active membership for Alice
        // Circle 2: Alice was a member, but left (LeftUtc is set)
        // Circle 3: Alice was never a member
        context.Circles!.AddRange(
            new Circle { CircleId = 1, Name = "Active Circle", CreatedByUserId = 10 },
            new Circle { CircleId = 2, Name = "Left Circle", CreatedByUserId = 20 },
            new Circle { CircleId = 3, Name = "Unjoined Circle", CreatedByUserId = 30 });

        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 10 },
            new CircleMembership { CircleId = 2, UserId = 10, LeftUtc = DateTime.UtcNow.AddDays(-1) },
            new CircleMembership { CircleId = 2, UserId = 20 },
            new CircleMembership { CircleId = 3, UserId = 30 });

        var now = DateTime.UtcNow;
        var activeEvent = new Event { EventId = 201, CircleId = 1, Title = "Active Circle Event", StartsUtc = now.AddDays(1), CreatedByUserId = 10 };
        var leftEvent = new Event { EventId = 202, CircleId = 2, Title = "Left Circle Event", StartsUtc = now.AddDays(2), CreatedByUserId = 20 };
        var unjoinedEvent = new Event { EventId = 203, CircleId = 3, Title = "Unjoined Circle Event", StartsUtc = now.AddDays(3), CreatedByUserId = 30 };

        context.Events!.AddRange(activeEvent, leftEvent, unjoinedEvent);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        // Alice's calendar query
        var aliceEvents = await repository.GetUpcomingEventsForUserAsync(10);

        Assert.Single(aliceEvents);
        Assert.Equal("Active Circle Event", aliceEvents[0].Title);
        Assert.Equal(1, aliceEvents[0].CircleId);
        Assert.DoesNotContain(aliceEvents, e => e.CircleId == 2);
        Assert.DoesNotContain(aliceEvents, e => e.CircleId == 3);

        // Bob's calendar query should still include Left Circle Event
        var bobEvents = await repository.GetUpcomingEventsForUserAsync(20);
        Assert.Single(bobEvents);
        Assert.Equal("Left Circle Event", bobEvents[0].Title);
    }

    // =========================================================================
    // 3. Recurrence expansion in calendar view
    // =========================================================================

    [Fact]
    public async Task CalendarView_ExpandsRecurringEventsAcrossMonthWindow()
    {
        await using var context = CreateContext();

        AddUser(context, 10, "Alice");
        context.Circles!.Add(new Circle { CircleId = 1, Name = "Fitness Circle", CreatedByUserId = 10 });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 10 });

        var monthStart = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        // One-off event on Sept 5
        var oneOffEvent = new Event
        {
            EventId = 301,
            CircleId = 1,
            Title = "Single Boot Camp",
            StartsUtc = new DateTime(2026, 9, 5, 9, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 5, 10, 0, 0, DateTimeKind.Utc),
            IsRecurring = false
        };

        // Weekly recurring event on Tuesdays starting Sept 1
        var weeklyEvent = new Event
        {
            EventId = 302,
            CircleId = 1,
            Title = "Weekly Tuesday Run",
            StartsUtc = new DateTime(2026, 9, 1, 7, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Weekly;DayOfWeek=Tuesday"
        };

        // Biweekly recurring event starting Sept 2 (Wednesdays: Sept 2, Sept 16, Sept 30)
        var biweeklyEvent = new Event
        {
            EventId = 303,
            CircleId = 1,
            Title = "Biweekly Yoga",
            StartsUtc = new DateTime(2026, 9, 2, 18, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 2, 19, 0, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Biweekly;DayOfWeek=Wednesday"
        };

        context.Events!.AddRange(oneOffEvent, weeklyEvent, biweeklyEvent);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);
        var recurrenceService = new RecurrenceService();

        // Get raw aggregated events for the circle member
        var rawEvents = await repository.GetEventsForCircleAsync(1, 10);
        Assert.Equal(3, rawEvents.Count);

        // Expand events into occurrences for the calendar month window
        var occurrences = recurrenceService.ExpandEvents(rawEvents, monthStart, monthEnd).ToList();

        // Sept 2026 has:
        // - 1 occurrence of Single Boot Camp (Sept 5)
        // - 5 Tuesdays (Sept 1, 8, 15, 22, 29) for Weekly Tuesday Run
        // - 3 Wednesdays (Sept 2, 16, 30) for Biweekly Yoga
        // Total = 1 + 5 + 3 = 9 occurrences
        Assert.Equal(9, occurrences.Count);

        // Verify occurrences are ordered chronologically by start time
        for (int i = 0; i < occurrences.Count - 1; i++)
        {
            Assert.True(occurrences[i].StartsUtc <= occurrences[i + 1].StartsUtc);
        }

        // Verify weekly occurrences
        var weeklyOccurrences = occurrences.Where(o => o.EventId == 302).ToList();
        Assert.Equal(5, weeklyOccurrences.Count);
        Assert.All(weeklyOccurrences, o => Assert.True(o.IsOccurrence));

        // Verify biweekly occurrences
        var biweeklyOccurrences = occurrences.Where(o => o.EventId == 303).ToList();
        Assert.Equal(3, biweeklyOccurrences.Count);
        Assert.Equal(new DateTime(2026, 9, 2, 18, 0, 0, DateTimeKind.Utc), biweeklyOccurrences[0].StartsUtc);
        Assert.Equal(new DateTime(2026, 9, 16, 18, 0, 0, DateTimeKind.Utc), biweeklyOccurrences[1].StartsUtc);
        Assert.Equal(new DateTime(2026, 9, 30, 18, 0, 0, DateTimeKind.Utc), biweeklyOccurrences[2].StartsUtc);

        // Verify one-off occurrence
        var oneOffOccurrences = occurrences.Where(o => o.EventId == 301).ToList();
        Assert.Single(oneOffOccurrences);
        Assert.False(oneOffOccurrences[0].IsOccurrence);
    }

    // =========================================================================
    // 4. RSVP status attachment on calendar events
    // =========================================================================

    [Fact]
    public async Task CalendarEvents_IncludeRsvpsAndAttachUserRsvpStatus()
    {
        await using var context = CreateContext();

        AddUser(context, 10, "Alice");
        AddUser(context, 20, "Bob");

        context.Circles!.Add(new Circle { CircleId = 1, Name = "Social Circle", CreatedByUserId = 10 });
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 10 },
            new CircleMembership { CircleId = 1, UserId = 20 });

        var now = DateTime.UtcNow;

        var eventAccepted = new Event { EventId = 401, CircleId = 1, Title = "Accepted Dinner", StartsUtc = now.AddDays(1), CreatedByUserId = 10 };
        var eventDeclined = new Event { EventId = 402, CircleId = 1, Title = "Declined Game", StartsUtc = now.AddDays(2), CreatedByUserId = 10 };
        var eventMaybe = new Event { EventId = 403, CircleId = 1, Title = "Maybe Hike", StartsUtc = now.AddDays(3), CreatedByUserId = 10 };
        var eventUnanswered = new Event { EventId = 404, CircleId = 1, Title = "Unanswered Concert", StartsUtc = now.AddDays(4), CreatedByUserId = 10 };

        context.Events!.AddRange(eventAccepted, eventDeclined, eventMaybe, eventUnanswered);
        await context.SaveChangesAsync();

        // Alice RSVPs
        context.Rsvps!.AddRange(
            new RSVP { EventId = 401, CircleId = 1, UserId = 10, Status = "Accept", Notes = "Can't wait!" },
            new RSVP { EventId = 402, CircleId = 1, UserId = 10, Status = "Decline", Notes = "Out of town" },
            new RSVP { EventId = 403, CircleId = 1, UserId = 10, Status = "Maybe", Notes = "Depends on weather" });
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var calendarEvents = await repository.GetUpcomingEventsForUserAsync(10);

        Assert.Equal(4, calendarEvents.Count);

        // Check Event 401 (Accepted)
        var acceptedItem = calendarEvents.First(e => e.EventId == 401);
        Assert.NotNull(acceptedItem.Rsvps);
        var aliceRsvp1 = acceptedItem.Rsvps.FirstOrDefault(r => r.UserId == 10);
        Assert.NotNull(aliceRsvp1);
        Assert.Equal("Accept", aliceRsvp1!.Status);
        Assert.Equal("Can't wait!", aliceRsvp1.Notes);

        // Check Event 402 (Declined)
        var declinedItem = calendarEvents.First(e => e.EventId == 402);
        var aliceRsvp2 = declinedItem.Rsvps.FirstOrDefault(r => r.UserId == 10);
        Assert.NotNull(aliceRsvp2);
        Assert.Equal("Decline", aliceRsvp2!.Status);
        Assert.Equal("Out of town", aliceRsvp2.Notes);

        // Check Event 403 (Maybe)
        var maybeItem = calendarEvents.First(e => e.EventId == 403);
        var aliceRsvp3 = maybeItem.Rsvps.FirstOrDefault(r => r.UserId == 10);
        Assert.NotNull(aliceRsvp3);
        Assert.Equal("Maybe", aliceRsvp3!.Status);
        Assert.Equal("Depends on weather", aliceRsvp3.Notes);

        // Check Event 404 (Unanswered)
        var unansweredItem = calendarEvents.First(e => e.EventId == 404);
        var aliceRsvp4 = unansweredItem.Rsvps.FirstOrDefault(r => r.UserId == 10);
        Assert.Null(aliceRsvp4); // No RSVP record attached for Alice
    }

    // Helper methods
    private static GetTogetherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GetTogetherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GetTogetherDbContext(options);
    }

    private static void AddUser(GetTogetherDbContext context, int userId, string displayName = null)
    {
        context.Users!.Add(new User
        {
            UserId = userId,
            ExternalId = $"user-{userId}",
            DisplayName = displayName ?? $"User {userId}",
            EmailAddress = $"user{userId}@example.com"
        });
        context.SaveChanges();
    }
}
