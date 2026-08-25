namespace DadABase.Tests;

using DadABase.Data;
using DadABase.Data.Models;
using DadABase.Data.Repositories;

[ExcludeFromCodeCoverage]
public class GetTogetherAccess_Tests
{
    [Fact]
    public async Task RedeemCode_AllowsOneNewMemberAndConsumesCode()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var repository = new InvitationCodeSQLRepository(context);

        var invitation = await repository.CreateCodeAsync(1, 10);
        var membership = await repository.RedeemCodeAsync(invitation.Code, 20);

        Assert.Equal(1, membership.CircleId);
        Assert.Equal(20, membership.UserId);
        Assert.Equal("Member", membership.Role);
        Assert.Null(await repository.FindValidCodeAsync(invitation.Code));
        Assert.Equal(20, (await context.InvitationCodes!.SingleAsync()).RedeemedByUserId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync(invitation.Code, 30));
    }

    [Fact]
    public async Task RedeemCode_RejectsInvalidExpiredAndUsedCodes()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.InvitationCodes!.AddRange(
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "expired", ExpiresUtc = DateTime.UtcNow.AddMinutes(-1) },
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "used", RedeemedByUserId = 99, RedeemedUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new InvitationCodeSQLRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync("missing", 20));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync("expired", 20));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync("used", 20));
    }

    [Fact]
    public async Task GetCodesForCircle_ReturnsRevokedCodesForMembers()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.InvitationCodes!.Add(new InvitationCode
        {
            CircleId = 1,
            CreatedByUserId = 10,
            Code = "revoked",
            RevokedUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var repository = new InvitationCodeSQLRepository(context);

        var codes = await repository.GetCodesForCircleAsync(1, 10);

        Assert.Contains(codes, invitation => invitation.Code == "revoked");
    }

    [Fact]
    public async Task RedeemCode_RejectsAnExistingActiveMember()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.InvitationCodes!.Add(new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "existing-member" });
        await context.SaveChangesAsync();
        var repository = new InvitationCodeSQLRepository(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync("existing-member", 10));
    }

    [Fact]
    public async Task CircleRepository_DeniesAccessFromAnotherCircle()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        SeedCircle(context, circleId: 2, creatorUserId: 20);
        var repository = new CircleSQLRepository(context);

        Assert.Null(await repository.GetCircleAsync(1, 20));
        Assert.Empty(await repository.GetMembersAsync(1, 20));
    }

    [Fact]
    public async Task CircleRepository_AddMember_RequiresActiveRequesterAndReactivatesMembership()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.Users!.Add(new User { UserId = 20, ExternalId = "user-20", DisplayName = "User 20" });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20, LeftUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new CircleSQLRepository(context);

        var membership = await repository.AddMemberAsync(1, 20, 10);

        Assert.Equal("Member", membership.Role);
        Assert.Null(membership.LeftUtc);
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddMemberAsync(1, 30, 30));
    }

    [Fact]
    public async Task CircleRepository_RemoveMember_DeletesMemberRsvps()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.Users!.Add(new User { UserId = 20, ExternalId = "user-20", DisplayName = "User 20" });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });
        context.Events!.Add(new Event { EventId = 1, CircleId = 1, CreatedByUserId = 10, Title = "Event", StartsUtc = DateTime.UtcNow });
        context.Rsvps!.Add(new RSVP { EventId = 1, CircleId = 1, UserId = 20 });
        await context.SaveChangesAsync();
        var repository = new CircleSQLRepository(context);

        await repository.RemoveMemberAsync(1, 20, 10);

        Assert.Empty(context.Rsvps!);
        Assert.Empty(await repository.GetMembersAsync(1, 20));
    }

    [Fact]
    public async Task InvitationCodes_AllowAnyActiveMemberAndReturnAllStatuses()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        context.Users!.Add(new User { UserId = 20, ExternalId = "user-20", DisplayName = "User 20" });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });
        await context.SaveChangesAsync();
        var repository = new InvitationCodeSQLRepository(context);

        var active = await repository.CreateCodeAsync(1, 20);
        context.InvitationCodes!.Add(new InvitationCode { CircleId = 1, CreatedByUserId = 20, Code = "expired", ExpiresUtc = DateTime.UtcNow.AddMinutes(-1) });
        await context.SaveChangesAsync();
        await repository.RevokeCodeAsync(active.InvitationCodeId, 20);

        var codes = await repository.GetCodesForCircleAsync(1, 20);

        Assert.Equal(2, codes.Count);
        Assert.Contains(codes, code => code.Code == active.Code && code.RevokedUtc is not null);
        Assert.Contains(codes, code => code.Code == "expired");
        Assert.Empty(await repository.GetCodesForCircleAsync(1, 20 + 1));
    }

    [Fact]
    public async Task CircleRepository_ListsAllActiveCirclesForUser()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        context.Circles!.Add(new Circle { CircleId = 2, Name = "Second Circle", CreatedByUserId = 20 });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 2, UserId = 20 });
        context.CircleMemberships.Add(new CircleMembership { CircleId = 1, UserId = 20 });
        await context.SaveChangesAsync();

        var circles = await new CircleSQLRepository(context).GetCirclesForUserAsync(20);

        Assert.Equal(["Circle 1", "Second Circle"], circles.Select(circle => circle.Name));
    }

    [Fact]
    public async Task CircleRepository_RosterContainsOnlyActiveMembersInDisplayOrder()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20, "Active Member");
        AddUser(context, 30, "Former Member");
        context.CircleMemberships!.AddRange(
            new CircleMembership { CircleId = 1, UserId = 20 },
            new CircleMembership { CircleId = 1, UserId = 30, LeftUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var members = await new CircleSQLRepository(context).GetMembersAsync(1, 10);

        Assert.Equal([20, 10], members.Select(member => member.UserId));
        Assert.DoesNotContain(members, member => member.UserId == 30);
        Assert.Equal("Active Member", members[0].User!.DisplayName);
    }

    [Fact]
    public async Task CircleScopedRepositories_DenyCrossCircleReadsAndWrites()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        SeedCircle(context, circleId: 2, creatorUserId: 20);
        context.Events!.Add(new Event { EventId = 1, CircleId = 1, Title = "Private event", StartsUtc = DateTime.UtcNow.AddDays(1), CreatedByUserId = 10 });
        context.InvitationCodes!.Add(new InvitationCode { InvitationCodeId = 1, CircleId = 1, CreatedByUserId = 10, Code = "private-invite" });
        await context.SaveChangesAsync();

        var eventRepository = new EventSQLRepository(context);
        var invitationRepository = new InvitationCodeSQLRepository(context);
        var rsvpRepository = new RsvpSQLRepository(context);

        Assert.Empty(await eventRepository.GetEventsForCircleAsync(1, 20));
        Assert.Null(await eventRepository.GetEventAsync(1, 20));
        Assert.Empty(await invitationRepository.GetCodesForCircleAsync(1, 20));
        Assert.Empty(await rsvpRepository.GetRsvpsForEventAsync(1, 20));
        await Assert.ThrowsAsync<InvalidOperationException>(() => eventRepository.CreateEventAsync(new Event { CircleId = 1, Title = "Denied", StartsUtc = DateTime.UtcNow.AddDays(2) }, 20));
        await Assert.ThrowsAsync<InvalidOperationException>(() => invitationRepository.CreateCodeAsync(1, 20));
        await Assert.ThrowsAsync<InvalidOperationException>(() => rsvpRepository.UpsertRsvpAsync(1, 20, "Accept"));
    }

    [Fact]
    public async Task InvitationRepository_AllowsUnlimitedCodesForActiveMembers()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });
        await context.SaveChangesAsync();
        var repository = new InvitationCodeSQLRepository(context);

        var codes = new List<InvitationCode>();
        for (var index = 0; index < 25; index++)
        {
            codes.Add(await repository.CreateCodeAsync(1, index % 2 == 0 ? 10 : 20));
        }

        Assert.Equal(25, codes.Select(code => code.Code).Distinct().Count());
        Assert.All(codes, code => Assert.Equal(1, code.CircleId));
    }

    [Fact]
    public async Task InvitationRepository_ListsActiveConsumedExpiredAndRevokedCodes()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        AddUser(context, 30);
        AddUser(context, 40);
        context.InvitationCodes!.AddRange(
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "active" },
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "consumed", RedeemedByUserId = 20, RedeemedUtc = DateTime.UtcNow },
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "expired", ExpiresUtc = DateTime.UtcNow.AddMinutes(-1) },
            new InvitationCode { CircleId = 1, CreatedByUserId = 10, Code = "revoked", RevokedUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var codes = await new InvitationCodeSQLRepository(context).GetCodesForCircleAsync(1, 10);

        Assert.Equal(["active", "consumed", "expired", "revoked"], codes.Select(code => code.Code).OrderBy(code => code));
    }

    [Fact]
    public async Task InvitationRepository_RevocationMakesCodeInvalid()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        var repository = new InvitationCodeSQLRepository(context);
        var invitation = await repository.CreateCodeAsync(1, 10);

        await repository.RevokeCodeAsync(invitation.InvitationCodeId, 10);

        Assert.Null(await repository.FindValidCodeAsync(invitation.Code));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.RedeemCodeAsync(invitation.Code, 20));
    }

    [Fact]
    public async Task CircleRepository_LeavingMemberIsSoftDeletedAndRemovesAccess()
    {
        await using var context = CreateContext();
        SeedCircle(context, circleId: 1, creatorUserId: 10);
        AddUser(context, 20);
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 20 });
        await context.SaveChangesAsync();
        var repository = new CircleSQLRepository(context);

        await repository.RemoveMemberAsync(1, 20, 10);

        Assert.NotNull(await context.CircleMemberships!.SingleAsync(member => member.CircleId == 1 && member.UserId == 20 && member.LeftUtc != null));
        Assert.DoesNotContain(await repository.GetMembersAsync(1, 10), member => member.UserId == 20);
        Assert.Empty(await repository.GetCirclesForUserAsync(20));
        Assert.Null(await repository.GetCircleAsync(1, 20));
    }

    [Fact]
    public async Task UserRepository_ResolvesExistingIdentityForRepeatOnboarding()
    {
        await using var context = CreateContext();
        var repository = new UserSQLRepository(context);
        var existingUser = await repository.CreateUserAsync(new User
        {
            ExternalId = "existing-provider-subject",
            DisplayName = "Existing User",
            EmailAddress = "existing@example.test"
        });

        var resolvedUser = await repository.FindByExternalIdAsync(existingUser.ExternalId);

        Assert.NotNull(resolvedUser);
        Assert.Equal(existingUser.UserId, resolvedUser!.UserId);
        Assert.Equal(existingUser.EmailAddress, resolvedUser.EmailAddress);
    }

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