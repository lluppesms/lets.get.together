//-----------------------------------------------------------------------
// <copyright file="RsvpRepository_Tests.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// RSVP Repository and Reminder Targeting Tests
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Tests;

using DadABase.Data;
using DadABase.Data.Models;
using DadABase.Data.Repositories;
using DadABase.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RsvpRepository_Tests
{
    // =========================================================================
    // 1. RSVP state transitions and attendance count aggregations
    // =========================================================================

    [Fact]
    public async Task UpsertRsvp_StateTransitions_AndCountAggregations()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Alice");
        AddUser(context, 30, "Bob");
        AddUser(context, 40, "Charlie");
        AddUser(context, 50, "Dave");
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 20 },
            new CircleMembership { CircleId = 1, UserId = 30 },
            new CircleMembership { CircleId = 1, UserId = 40 },
            new CircleMembership { CircleId = 1, UserId = 50 });

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Pickleball", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new RsvpSQLRepository(context);

        // State transition: Accept -> Decline -> Maybe -> Accept for User 20
        var rsvp1 = await repository.UpsertRsvpAsync(1, 20, "Accept", "Bringing balls");
        Assert.Equal("Accept", rsvp1.Status);
        Assert.Equal("Bringing balls", rsvp1.Notes);

        var rsvp2 = await repository.UpsertRsvpAsync(1, 20, "Decline", "Conflict");
        Assert.Equal("Decline", rsvp2.Status);
        Assert.Equal("Conflict", rsvp2.Notes);

        var rsvp3 = await repository.UpsertRsvpAsync(1, 20, "Maybe", "Might be late");
        Assert.Equal("Maybe", rsvp3.Status);

        var rsvp4 = await repository.UpsertRsvpAsync(1, 20, "Accept", "Cleared schedule!");
        Assert.Equal("Accept", rsvp4.Status);

        // Add RSVPs for other members
        await repository.UpsertRsvpAsync(1, 30, "Accept");
        await repository.UpsertRsvpAsync(1, 40, "Decline");
        // User 50 has not RSVP'd

        var rsvps = await repository.GetRsvpsForEventAsync(1, 10);
        Assert.Equal(3, rsvps.Count);

        // Aggregations check
        var acceptCount = rsvps.Count(r => r.Status == "Accept");
        var maybeCount = rsvps.Count(r => r.Status == "Maybe");
        var declineCount = rsvps.Count(r => r.Status == "Decline");

        Assert.Equal(2, acceptCount); // User 20 and User 30
        Assert.Equal(0, maybeCount);
        Assert.Equal(1, declineCount); // User 40

        // Transition User 40 from Decline to Maybe
        await repository.UpsertRsvpAsync(1, 40, "Maybe", "Pencil me in");
        var updatedRsvps = await repository.GetRsvpsForEventAsync(1, 10);

        Assert.Equal(2, updatedRsvps.Count(r => r.Status == "Accept"));
        Assert.Equal(1, updatedRsvps.Count(r => r.Status == "Maybe"));
        Assert.Equal(0, updatedRsvps.Count(r => r.Status == "Decline"));
    }

    [Fact]
    public async Task UpsertRsvp_RejectsInvalidStatusAndNonExistentEvent()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var repository = new RsvpSQLRepository(context);

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Coffee", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.UpsertRsvpAsync(1, 10, "Going"));
        await Assert.ThrowsAsync<ArgumentException>(() => repository.UpsertRsvpAsync(1, 10, "No"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpsertRsvpAsync(999, 10, "Accept"));
    }

    // =========================================================================
    // 2. Cross-circle RSVP denial
    // =========================================================================

    [Fact]
    public async Task RsvpRepository_DeniesCrossCircleRsvp_AndRsvpList()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        SeedCircle(context, circleId: 2, creatorUserId: 20);

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Circle 1 Private Event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new RsvpSQLRepository(context);

        // Non-member (User 20 in Circle 2) cannot RSVP to Event in Circle 1
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpsertRsvpAsync(1, 20, "Accept"));
        Assert.Contains("Only circle members may RSVP", ex.Message);

        // Non-member cannot view RSVPs for Event in Circle 1
        var rsvps = await repository.GetRsvpsForEventAsync(1, 20);
        Assert.Empty(rsvps);

        // Non-member cannot get unanswered members for Event in Circle 1
        var unanswered = await repository.GetUnansweredMembersAsync(1, 20);
        Assert.Empty(unanswered);
    }

    // =========================================================================
    // 3. Reminder targeting & circle membership enforcement
    // =========================================================================

    [Fact]
    public async Task GetUnansweredMembers_ReturnsOnlyUnansweredActiveCircleMembers()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Bob");
        AddUser(context, 30, "Charlie");
        AddUser(context, 40, "Alice");
        AddUser(context, 50, "Dave");
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 20 },
            new CircleMembership { CircleId = 1, UserId = 30 },
            new CircleMembership { CircleId = 1, UserId = 40 },
            new CircleMembership { CircleId = 1, UserId = 50 });

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Team Dinner", StartsUtc = DateTime.UtcNow.AddDays(2), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new RsvpSQLRepository(context);

        // User 20 and User 30 RSVP
        await repository.UpsertRsvpAsync(1, 20, "Accept");
        await repository.UpsertRsvpAsync(1, 30, "Decline");

        // Unanswered members for Event 1 (Creator 10, Alice 40, Dave 50)
        var unanswered = await repository.GetUnansweredMembersAsync(1, 10);

        Assert.Equal(3, unanswered.Count);
        var names = unanswered.Select(u => u.DisplayName).ToList();
        Assert.Contains("Alice", names);
        Assert.Contains("Dave", names);
        Assert.Contains("User 10", names);
        Assert.DoesNotContain("Bob", names);
        Assert.DoesNotContain("Charlie", names);

        // Verify NotificationService can process these unanswered recipients
        var config = new ConfigurationBuilder().Build();
        var notificationService = new SendGridNotificationService(config, NullLogger<SendGridNotificationService>.Instance);
        await notificationService.SendReminderAsync(ev, unanswered);
    }

    [Fact]
    public async Task GetUnansweredMembers_EnforcesCircleMembership()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        SeedCircle(context, circleId: 2, creatorUserId: 99);

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Circle 1 Event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new RsvpSQLRepository(context);

        // Non-member User 99 requesting unanswered list for Circle 1 Event
        var unanswered = await repository.GetUnansweredMembersAsync(1, 99);
        Assert.Empty(unanswered);
    }

    [Fact]
    public async Task GetRsvpsByOccurrence_And_GetRsvpsByEvent_EnforceActiveCircleMembership()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Alice");
        AddUser(context, 30, "NonMember");
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Weekly Coffee", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10, IsRecurring = true };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var repository = new RsvpSQLRepository(context);

        var occurrenceDate = DateTime.UtcNow.Date.AddDays(1);
        await repository.UpsertRsvpAsync(1, 20, "Accept", "See you there", occurrenceDate);

        // Active member (User 10) can view RSVPs by event and occurrence
        var eventRsvps = await repository.GetRsvpsByEventAsync(1, 10);
        Assert.Single(eventRsvps);
        Assert.Equal(20, eventRsvps[0].UserId);

        var occurrenceRsvps = await repository.GetRsvpsByOccurrenceAsync(1, occurrenceDate, 10);
        Assert.Single(occurrenceRsvps);
        Assert.Equal(20, occurrenceRsvps[0].UserId);

        // Occurrence search for a different date returns empty if no matching series or date RSVP
        var otherDateRsvps = await repository.GetRsvpsByOccurrenceAsync(1, occurrenceDate.AddDays(7), 10);
        Assert.Empty(otherDateRsvps);

        // Non-member (User 30) gets empty list
        var nonMemberEventRsvps = await repository.GetRsvpsByEventAsync(1, 30);
        Assert.Empty(nonMemberEventRsvps);

        var nonMemberOccurrenceRsvps = await repository.GetRsvpsByOccurrenceAsync(1, occurrenceDate, 30);
        Assert.Empty(nonMemberOccurrenceRsvps);
    }

    [Fact]
    public async Task SendReminderEmailAsync_EnforcesCircleMembership_AndLogsToDatabase()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Member Bob");
        AddUser(context, 30, "NonMember Charlie");
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });

        var ev = new Event { EventId = 1, CircleId = 1, Title = "Planning Session", StartsUtc = DateTime.UtcNow.AddDays(2), CreatedByUserId = 10 };
        context.Events!.Add(ev);
        await context.SaveChangesAsync();

        var config = new ConfigurationBuilder().Build();
        var notificationService = new SendGridNotificationService(config, NullLogger<SendGridNotificationService>.Instance, context);

        var memberBob = await context.Users!.FindAsync(20);
        var nonMemberCharlie = await context.Users!.FindAsync(30);

        // Non-member (User 30) trying to trigger reminder per OQ-1 is blocked
        await notificationService.SendReminderEmailAsync(ev, [memberBob!], triggeringUserId: 30);
        Assert.Empty(await context.ReminderLogs!.ToListAsync());

        // Active member (User 10) triggering reminder sends to valid circle members and logs to DB
        await notificationService.SendReminderEmailAsync(ev, [memberBob!, nonMemberCharlie!], triggeringUserId: 10);

        var logs = await context.ReminderLogs!.ToListAsync();
        Assert.Single(logs); // Non-member Charlie was filtered out from target audience
        Assert.Equal(1, logs[0].EventId);
        Assert.Equal(20, logs[0].UserId);
        Assert.Equal("Email", logs[0].Channel);
        Assert.Equal("Logged", logs[0].DeliveryState);
    }

    // =========================================================================
    // 4. Member leave cleanup
    // =========================================================================

    [Fact]
    public async Task MemberLeave_PurgesMemberRsvps_AndUpdatesUnansweredList()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Leaving User");
        AddUser(context, 30, "Staying User");
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 20 },
            new CircleMembership { CircleId = 1, UserId = 30 });

        var ev1 = new Event { EventId = 1, CircleId = 1, Title = "Event 1", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 };
        var ev2 = new Event { EventId = 2, CircleId = 1, Title = "Event 2", StartsUtc = DateTime.UtcNow.AddDays(3), CreatedByUserId = 10 };
        context.Events!.AddRange(ev1, ev2);
        await context.SaveChangesAsync();

        var rsvpRepo = new RsvpSQLRepository(context);
        var circleRepo = new CircleSQLRepository(context);

        // User 20 RSVPs to Event 1 and Event 2; User 30 RSVPs to Event 1
        await rsvpRepo.UpsertRsvpAsync(1, 20, "Accept");
        await rsvpRepo.UpsertRsvpAsync(2, 20, "Maybe");
        await rsvpRepo.UpsertRsvpAsync(1, 30, "Accept");

        Assert.Equal(2, (await context.Rsvps!.ToListAsync()).Count(r => r.UserId == 20));

        // User 20 leaves circle
        await circleRepo.RemoveMemberAsync(1, 20, 20);

        // Verify membership is soft-deleted
        var membership = await context.CircleMemberships!.SingleAsync(m => m.CircleId == 1 && m.UserId == 20);
        Assert.NotNull(membership.LeftUtc);

        // Verify User 20's RSVPs in Circle 1 were purged
        var remainingRsvps = await context.Rsvps!.ToListAsync();
        Assert.DoesNotContain(remainingRsvps, r => r.UserId == 20);

        // User 30's RSVP remains intact
        var ev1Rsvps = await rsvpRepo.GetRsvpsForEventAsync(1, 10);
        Assert.Single(ev1Rsvps);
        Assert.Equal(30, ev1Rsvps[0].UserId);

        // Unanswered members list for Event 1 no longer includes departed User 20
        var unanswered = await rsvpRepo.GetUnansweredMembersAsync(1, 10);
        Assert.Single(unanswered);
        Assert.Equal(10, unanswered[0].UserId); // Only Creator 10 hasn't RSVP'd
    }

    // =========================================================================
    // Test Context & Helpers
    // =========================================================================

    private static DadABaseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DadABaseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DadABaseDbContext(options);
    }

    private static void SeedCircle(DadABaseDbContext context, int circleId, int creatorUserId)
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

    private static void AddUser(DadABaseDbContext context, int userId, string displayName = null)
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
