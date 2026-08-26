//-----------------------------------------------------------------------
// <copyright file="EventRepository_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Event Repository and Recurrence Expansion Tests
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Helpers;

namespace GetTogether.Tests;

[ExcludeFromCodeCoverage]
public class EventRepository_Tests
{
    // =========================================================================
    // 1. Event creation, retrieval, and updates
    // =========================================================================

    [Fact]
    public async Task CreateEvent_PersistsEvent_ForCircleMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var repository = new EventSQLRepository(context);

        var newEvent = new Event
        {
            CircleId = 1,
            Title = "Pickleball Match",
            Details = "Bring paddles and water",
            StartsUtc = DateTime.UtcNow.AddDays(1),
            EndsUtc = DateTime.UtcNow.AddDays(1).AddHours(2),
            IsRecurring = false
        };

        var created = await repository.CreateEventAsync(newEvent, 10);

        Assert.True(created.EventId > 0);
        Assert.Equal("Pickleball Match", created.Title);
        Assert.Equal(10, created.CreatedByUserId);
        Assert.True(created.CreatedUtc <= DateTime.UtcNow);

        var fetched = await context.Events!.FindAsync(created.EventId);
        Assert.NotNull(fetched);
        Assert.Equal("Pickleball Match", fetched!.Title);
    }

    [Fact]
    public async Task GetEvent_ReturnsEvent_WithRsvps_ForCircleMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });

        var ev = new Event
        {
            CircleId = 1,
            Title = "Movie Night",
            StartsUtc = DateTime.UtcNow.AddDays(2),
            CreatedByUserId = 10
        };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        context.Rsvps!.Add(new RSVP { EventId = ev.EventId, CircleId = 1, UserId = 20, Status = "Accept" });
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var retrieved = await repository.GetEventAsync(ev.EventId, 10);

        Assert.NotNull(retrieved);
        Assert.Equal("Movie Night", retrieved!.Title);
        Assert.Single(retrieved.Rsvps);
        Assert.Equal("Accept", retrieved.Rsvps.First().Status);
    }

    [Fact]
    public async Task GetEventsForCircle_ReturnsActiveEvents_OrderedByStart()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var repository = new EventSQLRepository(context);

        var now = DateTime.UtcNow;
        var laterEvent = new Event { CircleId = 1, Title = "Later", StartsUtc = now.AddDays(5), CreatedByUserId = 10 };
        var earlierEvent = new Event { CircleId = 1, Title = "Earlier", StartsUtc = now.AddDays(1), CreatedByUserId = 10 };
        var cancelledEvent = new Event { CircleId = 1, Title = "Cancelled", StartsUtc = now.AddDays(2), CreatedByUserId = 10, CancelledUtc = now };

        context.Events!.AddRange(laterEvent, earlierEvent, cancelledEvent);
        await context.SaveChangesAsync();

        var events = await repository.GetEventsForCircleAsync(1, 10);

        Assert.Equal(2, events.Count);
        Assert.Equal("Earlier", events[0].Title);
        Assert.Equal("Later", events[1].Title);
        Assert.DoesNotContain(events, e => e.Title == "Cancelled");
    }

    [Fact]
    public async Task GetUpcomingEventsForUser_ReturnsEventsAcrossUserCircles()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        context.Circles!.Add(new Circle { CircleId = 2, Name = "Circle 2", CreatedByUserId = 20 });
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 20 },
            new CircleMembership { CircleId = 2, UserId = 20 });

        var now = DateTime.UtcNow;
        context.Events!.AddRange(
            new Event { CircleId = 1, Title = "Circle 1 Event", StartsUtc = now.AddDays(1), CreatedByUserId = 10 },
            new Event { CircleId = 2, Title = "Circle 2 Event", StartsUtc = now.AddDays(3), CreatedByUserId = 20 },
            new Event { CircleId = 1, Title = "Past Event", StartsUtc = now.AddDays(-2), CreatedByUserId = 10 });
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var upcoming = await repository.GetUpcomingEventsForUserAsync(20);

        Assert.Equal(2, upcoming.Count);
        Assert.Equal("Circle 1 Event", upcoming[0].Title);
        Assert.Equal("Circle 2 Event", upcoming[1].Title);
    }

    [Fact]
    public async Task UpdateEvent_UpdatesFields_ForCircleMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var ev = new Event
        {
            CircleId = 1,
            Title = "Original Title",
            Details = "Original Details",
            StartsUtc = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = 10
        };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        ev.Title = "Updated Title";
        ev.Details = "Updated Details";
        ev.IsRecurring = true;
        ev.RecurrenceRule = "Weekly";
        ev.RsvpMode = RsvpMode.Series;

        await repository.UpdateEventAsync(ev, 10);

        var updated = await context.Events!.FindAsync(ev.EventId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated!.Title);
        Assert.Equal("Updated Details", updated.Details);
        Assert.True(updated.IsRecurring);
        Assert.Equal("Weekly", updated.RecurrenceRule);
        Assert.Equal(RsvpMode.Series, updated.RsvpMode);
    }

    [Fact]
    public async Task CancelEvent_SetsCancelledUtc_ForCircleMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var ev = new Event
        {
            CircleId = 1,
            Title = "Event To Cancel",
            StartsUtc = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = 10
        };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        await repository.CancelEventAsync(ev.EventId, 10);

        var cancelled = await context.Events!.FindAsync(ev.EventId);
        Assert.NotNull(cancelled);
        Assert.NotNull(cancelled!.CancelledUtc);

        var activeEvents = await repository.GetEventsForCircleAsync(1, 10);
        Assert.Empty(activeEvents);
    }

    // =========================================================================
    // 2. Circle-member access guards
    // =========================================================================

    [Fact]
    public async Task GetEvent_ReturnsNull_ForNonMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 99); // Non-member

        var ev = new Event { CircleId = 1, Title = "Secret Event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var retrieved = await repository.GetEventAsync(ev.EventId, 99);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetEventsForCircle_ReturnsEmpty_ForNonMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 99);

        context.Events!.Add(new Event { CircleId = 1, Title = "Private Event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 });
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        var events = await repository.GetEventsForCircleAsync(1, 99);

        Assert.Empty(events);
    }

    [Fact]
    public async Task CreateEvent_ThrowsInvalidOperationException_ForNonMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 99);

        var repository = new EventSQLRepository(context);
        var newEvent = new Event { CircleId = 1, Title = "Unauthorized Event", StartsUtc = DateTime.UtcNow.AddDays(1) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CreateEventAsync(newEvent, 99));
    }

    [Fact]
    public async Task UpdateEvent_ThrowsInvalidOperationException_ForNonMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 99);

        var ev = new Event { CircleId = 1, Title = "Original", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);
        ev.Title = "Hacked Title";

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateEventAsync(ev, 99));
    }

    [Fact]
    public async Task CancelEvent_ThrowsInvalidOperationException_ForNonMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 99);

        var ev = new Event { CircleId = 1, Title = "Protected Event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new EventSQLRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CancelEventAsync(ev.EventId, 99));
    }

    // =========================================================================
    // 3. Recurrence expansion unit tests (Weekly, Biweekly, Monthly)
    // =========================================================================

    [Fact]
    public void ExpandOccurrences_Weekly_GeneratesSevenDayIntervals()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddDays(28); // 4 weeks

        var @event = new Event
        {
            StartsUtc = startUtc,
            IsRecurring = true,
            RecurrenceRule = "Weekly"
        };

        var occurrences = RecurrenceExpander.ExpandOccurrences(@event, untilUtc);

        Assert.Equal(5, occurrences.Count);
        Assert.Equal(startUtc, occurrences[0]);
        Assert.Equal(startUtc.AddDays(7), occurrences[1]);
        Assert.Equal(startUtc.AddDays(14), occurrences[2]);
        Assert.Equal(startUtc.AddDays(21), occurrences[3]);
        Assert.Equal(startUtc.AddDays(28), occurrences[4]);
    }

    [Fact]
    public void ExpandOccurrences_Weekly_RRuleFormat_GeneratesCorrectDates()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddDays(21);

        var occurrences = RecurrenceExpander.ExpandOccurrences(
            startUtc, isRecurring: true, recurrenceRule: "FREQ=WEEKLY", untilUtc: untilUtc);

        Assert.Equal(4, occurrences.Count);
        Assert.Equal(startUtc, occurrences[0]);
        Assert.Equal(startUtc.AddDays(7), occurrences[1]);
        Assert.Equal(startUtc.AddDays(14), occurrences[2]);
        Assert.Equal(startUtc.AddDays(21), occurrences[3]);
    }

    [Fact]
    public void ExpandOccurrences_Biweekly_GeneratesFourteenDayIntervals()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddDays(56); // 8 weeks -> 5 occurrences (start, +14, +28, +42, +56)

        var @event = new Event
        {
            StartsUtc = startUtc,
            IsRecurring = true,
            RecurrenceRule = "Biweekly"
        };

        var occurrences = RecurrenceExpander.ExpandOccurrences(@event, untilUtc);

        Assert.Equal(5, occurrences.Count);
        Assert.Equal(startUtc, occurrences[0]);
        Assert.Equal(startUtc.AddDays(14), occurrences[1]);
        Assert.Equal(startUtc.AddDays(28), occurrences[2]);
        Assert.Equal(startUtc.AddDays(42), occurrences[3]);
        Assert.Equal(startUtc.AddDays(56), occurrences[4]);
    }

    [Fact]
    public void ExpandOccurrences_Biweekly_RRuleIntervalFormat_GeneratesCorrectDates()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddDays(30);

        var occurrences = RecurrenceExpander.ExpandOccurrences(
            startUtc, isRecurring: true, recurrenceRule: "FREQ=WEEKLY;INTERVAL=2", untilUtc: untilUtc);

        Assert.Equal(3, occurrences.Count);
        Assert.Equal(startUtc, occurrences[0]);
        Assert.Equal(startUtc.AddDays(14), occurrences[1]);
        Assert.Equal(startUtc.AddDays(28), occurrences[2]);
    }

    [Fact]
    public void ExpandOccurrences_Monthly_GeneratesOneMonthIntervals()
    {
        var startUtc = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddMonths(4);

        var @event = new Event
        {
            StartsUtc = startUtc,
            IsRecurring = true,
            RecurrenceRule = "Monthly"
        };

        var occurrences = RecurrenceExpander.ExpandOccurrences(@event, untilUtc);

        Assert.Equal(5, occurrences.Count);
        Assert.Equal(startUtc, occurrences[0]);
        Assert.Equal(new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc), occurrences[1]);
        Assert.Equal(new DateTime(2026, 11, 15, 14, 0, 0, DateTimeKind.Utc), occurrences[2]);
        Assert.Equal(new DateTime(2026, 12, 15, 14, 0, 0, DateTimeKind.Utc), occurrences[3]);
        Assert.Equal(new DateTime(2027, 1, 15, 14, 0, 0, DateTimeKind.Utc), occurrences[4]);
    }

    [Fact]
    public void ExpandOccurrences_NonRecurring_ReturnsSingleOccurrence()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddDays(30);

        var @event = new Event
        {
            StartsUtc = startUtc,
            IsRecurring = false,
            RecurrenceRule = null
        };

        var occurrences = RecurrenceExpander.ExpandOccurrences(@event, untilUtc);

        Assert.Single(occurrences);
        Assert.Equal(startUtc, occurrences[0]);
    }

    [Fact]
    public void ExpandOccurrences_RespectsMaxCount()
    {
        var startUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
        var untilUtc = startUtc.AddYears(1); // Long range

        var @event = new Event
        {
            StartsUtc = startUtc,
            IsRecurring = true,
            RecurrenceRule = "Weekly"
        };

        var occurrences = RecurrenceExpander.ExpandOccurrences(@event, untilUtc, maxCount: 3);

        Assert.Equal(3, occurrences.Count);
    }

    // Helper methods
    private static GetTogetherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GetTogetherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GetTogetherDbContext(options);
    }

    private static void SeedCircle(GetTogetherDbContext context, int circleId, int creatorUserId)
    {
        AddUser(context, creatorUserId);
        context.Circles!.Add(new Circle
        {
            CircleId = circleId,
            Name = $"Circle {circleId}",
            CreatedByUserId = creatorUserId
        });
        context.CircleMemberships!.Add(new CircleMembership
        {
            CircleId = circleId,
            UserId = creatorUserId
        });
        context.SaveChanges();
    }

    private static void AddUser(GetTogetherDbContext context, int userId, string displayName = null)
    {
        context.Users!.Add(new User
        {
            UserId = userId,
            ExternalId = $"user-{userId}",
            DisplayName = displayName ?? $"User {userId}",
            EmailAddress = $"user{userId}@example.test"
        });
    }
}
