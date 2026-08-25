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
        context.Users!.Add(new User
        {
            UserId = creatorUserId,
            ExternalId = $"user-{creatorUserId}",
            DisplayName = $"User {creatorUserId}",
            EmailAddress = $"user{creatorUserId}@example.test"
        });
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
}