//-----------------------------------------------------------------------
// <copyright file="CalendarAggregationService_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Calendar Aggregation Service Tests
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
public class CalendarAggregationService_Tests
{
    private static GetTogetherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GetTogetherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GetTogetherDbContext(options);
    }

    private static void SeedUser(GetTogetherDbContext context, int userId, string name)
    {
        context.Users!.Add(new User
        {
            UserId = userId,
            ExternalId = $"ext_{userId}",
            DisplayName = name,
            EmailAddress = $"{name.ToLowerInvariant()}@example.com"
        });
        context.SaveChanges();
    }

    private static void SeedCircle(GetTogetherDbContext context, int circleId, string name, int creatorUserId)
    {
        context.Circles!.Add(new Circle
        {
            CircleId = circleId,
            Name = name,
            CreatedByUserId = creatorUserId,
            CreatedUtc = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetCalendarEventsForUserAsync_MultiCircleAggregation_AttachesCircleMetadataAndRsvps()
    {
        await using var context = CreateContext();

        // Seed user 10 (Alice) and user 20 (Bob)
        SeedUser(context, 10, "Alice");
        SeedUser(context, 20, "Bob");

        // Circle 1: Pickleball Crew, Circle 2: Book Club
        SeedCircle(context, 1, "Pickleball Crew", 10);
        SeedCircle(context, 2, "Book Club", 20);

        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 10 },
            new CircleMembership { CircleId = 2, UserId = 10, JoinedUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Circle 1 One-off Event
        var ev1 = new Event
        {
            EventId = 101,
            CircleId = 1,
            Title = "Friday Pickleball Match",
            StartsUtc = new DateTime(2026, 9, 11, 10, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        // Circle 2 One-off Event
        var ev2 = new Event
        {
            EventId = 102,
            CircleId = 2,
            Title = "Monthly Book Discussion",
            StartsUtc = new DateTime(2026, 9, 15, 19, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 20
        };

        context.Events!.AddRange(ev1, ev2);
        await context.SaveChangesAsync();

        // Alice RSVPs Accept to Pickleball Match (Event 101)
        var rsvpRepo = new RsvpSQLRepository(context);
        await rsvpRepo.UpsertRsvpAsync(101, 10, "Accept", "Bringing extra paddles");

        // Service composition
        var circleRepo = new CircleSQLRepository(context);
        var eventRepo = new EventSQLRepository(context);
        var recurrenceService = new RecurrenceService();
        var aggregationService = new CalendarAggregationService(circleRepo, eventRepo, rsvpRepo, recurrenceService);

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var items = await aggregationService.GetCalendarEventsForUserAsync(10, startWindow, endWindow);

        Assert.Equal(2, items.Count);

        // First event (Sept 11)
        Assert.Equal("Friday Pickleball Match", items[0].Title);
        Assert.Equal("Pickleball Crew", items[0].CircleName);
        Assert.Equal("Accept", items[0].UserRsvpStatus);
        Assert.Equal("Bringing extra paddles", items[0].UserRsvpNotes);

        // Second event (Sept 15)
        Assert.Equal("Monthly Book Discussion", items[1].Title);
        Assert.Equal("Book Club", items[1].CircleName);
        Assert.Equal("Unanswered", items[1].UserRsvpStatus);

        // Verify distinct circle metadata / color indices
        Assert.NotEqual(items[0].CircleId, items[1].CircleId);
        Assert.False(string.IsNullOrWhiteSpace(items[0].CircleColor));
        Assert.False(string.IsNullOrWhiteSpace(items[1].CircleColor));
    }

    [Fact]
    public async Task GetCalendarEventsForUserAsync_DateWindowBounds_ExcludesEventsOutsideWindow()
    {
        await using var context = CreateContext();
        SeedUser(context, 10, "Alice");
        SeedCircle(context, 1, "Running Club", 10);
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 10 });

        // Event before window
        var evPast = new Event
        {
            EventId = 1,
            CircleId = 1,
            Title = "August Run",
            StartsUtc = new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        // Event inside window
        var evSept = new Event
        {
            EventId = 2,
            CircleId = 1,
            Title = "September Run",
            StartsUtc = new DateTime(2026, 9, 10, 8, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        // Event after window
        var evFuture = new Event
        {
            EventId = 3,
            CircleId = 1,
            Title = "October Run",
            StartsUtc = new DateTime(2026, 10, 5, 8, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        context.Events!.AddRange(evPast, evSept, evFuture);
        await context.SaveChangesAsync();

        var aggregationService = new CalendarAggregationService(
            new CircleSQLRepository(context),
            new EventSQLRepository(context),
            new RsvpSQLRepository(context),
            new RecurrenceService());

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var items = await aggregationService.GetCalendarEventsForUserAsync(10, startWindow, endWindow);

        Assert.Single(items);
        Assert.Equal("September Run", items[0].Title);
        Assert.Equal(evSept.StartsUtc, items[0].StartsUtc);
    }

    [Fact]
    public async Task GetCalendarEventsForUserAsync_RecurrenceExpansion_ExpandsRecurringEventOccurrences()
    {
        await using var context = CreateContext();
        SeedUser(context, 10, "Alice");
        SeedCircle(context, 1, "Yoga Group", 10);
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 10 });

        // Weekly Tuesday event starting Sept 1, 2026
        var recurringEvent = new Event
        {
            EventId = 201,
            CircleId = 1,
            Title = "Weekly Morning Yoga",
            StartsUtc = new DateTime(2026, 9, 1, 7, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Weekly;DayOfWeek=Tuesday",
            RsvpMode = RsvpMode.PerOccurrence,
            CreatedByUserId = 10
        };

        context.Events!.Add(recurringEvent);
        await context.SaveChangesAsync();

        // Alice RSVPs Accept specifically for occurrence on Sept 15, 2026
        var rsvpRepo = new RsvpSQLRepository(context);
        var Sept15Date = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);
        await rsvpRepo.UpsertRsvpAsync(201, 10, "Accept", "Attending this week", Sept15Date);

        var aggregationService = new CalendarAggregationService(
            new CircleSQLRepository(context),
            new EventSQLRepository(context),
            rsvpRepo,
            new RecurrenceService());

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var items = await aggregationService.GetCalendarEventsForUserAsync(10, startWindow, endWindow);

        // Sept 1, 8, 15, 22, 29 = 5 occurrences in Sept 2026
        Assert.Equal(5, items.Count);
        Assert.All(items, item =>
        {
            Assert.True(item.IsRecurring);
            Assert.True(item.IsOccurrence);
            Assert.Equal("Yoga Group", item.CircleName);
        });

        // Occurrence on Sept 15 should show "Accept"
        var Sept15Item = items.First(i => i.StartsUtc.Date == Sept15Date);
        Assert.Equal("Accept", Sept15Item.UserRsvpStatus);
        Assert.Equal("Attending this week", Sept15Item.UserRsvpNotes);

        // Other occurrences should show "Unanswered"
        var Sept1Item = items.First(i => i.StartsUtc.Date == new DateTime(2026, 9, 1));
        Assert.Equal("Unanswered", Sept1Item.UserRsvpStatus);
    }

    [Fact]
    public async Task GetCalendarEventsForUserAsync_FormerMemberExclusion_StrictCirclePrivacyEnforced()
    {
        await using var context = CreateContext();
        SeedUser(context, 10, "Alice");

        // Circle 1 (Active member) and Circle 2 (Former member)
        SeedCircle(context, 1, "Active Crew", 10);
        SeedCircle(context, 2, "Departed Crew", 10);

        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 10, LeftUtc = null },
            new CircleMembership { CircleId = 2, UserId = 10, LeftUtc = DateTime.UtcNow.AddDays(-5) });
        await context.SaveChangesAsync();

        var evActive = new Event
        {
            EventId = 1,
            CircleId = 1,
            Title = "Active Event",
            StartsUtc = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        var evDeparted = new Event
        {
            EventId = 2,
            CircleId = 2,
            Title = "Departed Circle Event",
            StartsUtc = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc),
            CreatedByUserId = 10
        };

        context.Events!.AddRange(evActive, evDeparted);
        await context.SaveChangesAsync();

        var aggregationService = new CalendarAggregationService(
            new CircleSQLRepository(context),
            new EventSQLRepository(context),
            new RsvpSQLRepository(context),
            new RecurrenceService());

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var items = await aggregationService.GetCalendarEventsForUserAsync(10, startWindow, endWindow);

        Assert.Single(items);
        Assert.Equal("Active Event", items[0].Title);
        Assert.Equal("Active Crew", items[0].CircleName);
        Assert.DoesNotContain(items, i => i.CircleId == 2);
    }

    [Fact]
    public async Task GetCalendarEventsForUserAsync_InvalidDateWindow_ThrowsArgumentException()
    {
        await using var context = CreateContext();

        var aggregationService = new CalendarAggregationService(
            new CircleSQLRepository(context),
            new EventSQLRepository(context),
            new RsvpSQLRepository(context),
            new RecurrenceService());

        var startWindow = DateTime.UtcNow;
        var endWindow = startWindow.AddDays(-1);

        await Assert.ThrowsAsync<ArgumentException>(() => aggregationService.GetCalendarEventsForUserAsync(10, startWindow, endWindow));
    }
}
