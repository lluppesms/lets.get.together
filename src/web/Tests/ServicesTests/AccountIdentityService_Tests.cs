using GetTogether.Data;
using GetTogether.Data.Models;
using GetTogether.Data.Services;

namespace GetTogether.Tests;

public class AccountIdentityService_Tests
{
    [Fact]
    public async Task CompleteOnboardingAsync_WithVerifiedEmailBoundInvite_CreatesUserAndConsumesRecords()
    {
        await using var context = CreateContext();
        SeedCircle(context);
        var sender = new CapturingVerificationEmailSender();
        var service = new AccountIdentityService(context, sender);
        var invitation = await service.CreateInvitationAsync(1, 10, "new.member@example.test");

        await service.BeginInvitationVerificationAsync(invitation.Code, "new.member@example.test");
        var user = await service.CompleteOnboardingAsync(
            invitation.Code,
            "new.member@example.test",
            sender.Token!,
            "New Member",
            new ExternalIdentityInput(ExternalIdentityProvider.Google, "https://accounts.google.com", "google-subject"));

        Assert.Equal("New Member", user.DisplayName);
        Assert.True((await context.UserEmailAliases!.SingleAsync(alias => alias.UserId == user.UserId)).IsPrimary);
        Assert.NotNull((await context.InvitationCodes!.SingleAsync()).RedeemedUtc);
        Assert.NotNull((await context.EmailVerificationTokens!.SingleAsync()).UsedUtc);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteOnboardingAsync(
            invitation.Code,
            "new.member@example.test",
            sender.Token!,
            "Another Member",
            new ExternalIdentityInput(ExternalIdentityProvider.Facebook, "https://www.facebook.com", "facebook-subject")));
    }

    [Fact]
    public async Task CompleteOnboardingAsync_WhenInviteEmailDiffers_RejectsWithoutCreatingUser()
    {
        await using var context = CreateContext();
        SeedCircle(context);
        var sender = new CapturingVerificationEmailSender();
        var service = new AccountIdentityService(context, sender);
        var invitation = await service.CreateInvitationAsync(1, 10, "bound@example.test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.BeginInvitationVerificationAsync(invitation.Code, "other@example.test"));

        Assert.Equal(1, await context.Users!.CountAsync());
    }

    [Fact]
    public async Task UnlinkIdentityAsync_WhenIdentityIsLast_Rejects()
    {
        await using var context = CreateContext();
        var user = new User { DisplayName = "Member" };
        user.Identities.Add(new UserIdentity { Provider = ExternalIdentityProvider.Entra, Issuer = "https://login.example.test", Subject = "subject" });
        context.Users!.Add(user);
        await context.SaveChangesAsync();
        var service = new AccountIdentityService(context, new CapturingVerificationEmailSender());
        var identityId = (await context.UserIdentities!.SingleAsync()).UserIdentityId;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UnlinkIdentityAsync(user.UserId, identityId, recentlyAuthenticated: true));

        Assert.Equal(1, await context.UserIdentities.CountAsync());
    }

    [Fact]
    public async Task VerifyEmailAliasAsync_WithDeliveredToken_VerifiesAlias()
    {
        await using var context = CreateContext();
        var user = new User { DisplayName = "Member" };
        context.Users!.Add(user);
        await context.SaveChangesAsync();
        var sender = new CapturingVerificationEmailSender();
        var service = new AccountIdentityService(context, sender);

        await service.AddEmailAliasAsync(user.UserId, "member@example.test");
        var alias = await context.UserEmailAliases!.SingleAsync();
        await service.VerifyEmailAliasAsync(user.UserId, alias.UserEmailAliasId, sender.Token!);

        Assert.True(alias.IsVerified);
    }

    private static GetTogetherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GetTogetherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GetTogetherDbContext(options);
    }

    private static void SeedCircle(GetTogetherDbContext context)
    {
        context.Users!.Add(new User { UserId = 10, DisplayName = "Creator" });
        context.Circles!.Add(new Circle { CircleId = 1, Name = "Circle", CreatedByUserId = 10 });
        context.CircleMemberships!.Add(new CircleMembership { CircleId = 1, UserId = 10, Role = "Member" });
        context.SaveChanges();
    }

    private sealed class CapturingVerificationEmailSender : IVerificationEmailSender
    {
        public string? Token { get; private set; }

        public Task SendAsync(string emailAddress, string token, CancellationToken cancellationToken = default)
        {
            Token = token;
            return Task.CompletedTask;
        }
    }
}