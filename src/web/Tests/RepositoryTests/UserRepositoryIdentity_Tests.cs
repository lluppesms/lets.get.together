namespace GetTogether.Tests;

using GetTogether.Data;
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;

public class UserRepositoryIdentity_Tests
{
    [Fact]
    public async Task FindByIdentityAsync_ResolvesCanonicalUserRegardlessOfVerifiedAlias()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        var user = await repository.CreateUserAsync(
            new User { DisplayName = "Canonical user" },
            new UserIdentity { Provider = ExternalIdentityProvider.Google, Issuer = "https://accounts.google.com", Subject = "google-subject" },
            new UserEmailAlias { EmailAddress = "first@example.test", NormalizedEmailAddress = "FIRST@EXAMPLE.TEST", IsVerified = true });
        await repository.AddEmailAliasAsync(new UserEmailAlias
        {
            UserId = user.UserId,
            EmailAddress = "notification@example.test",
            NormalizedEmailAddress = "NOTIFICATION@EXAMPLE.TEST",
            IsVerified = true
        });

        var resolvedUser = await repository.FindByIdentityAsync(ExternalIdentityProvider.Google, "https://accounts.google.com", "google-subject");

        Assert.NotNull(resolvedUser);
        Assert.Equal(user.UserId, resolvedUser!.UserId);
        Assert.Equal(2, resolvedUser.EmailAliases.Count);
    }

    [Fact]
    public async Task AddIdentityAsync_LinksAnotherProviderToTheExistingCanonicalUser()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        var user = await repository.CreateUserAsync(
            new User { DisplayName = "Linked user" },
            new UserIdentity { Provider = ExternalIdentityProvider.Entra, Issuer = "https://login.microsoftonline.com/tenant/v2.0", Subject = "entra-subject" },
            new UserEmailAlias { EmailAddress = "linked@example.test", NormalizedEmailAddress = "LINKED@EXAMPLE.TEST", IsVerified = true });

        await repository.AddIdentityAsync(new UserIdentity
        {
            UserId = user.UserId,
            Provider = ExternalIdentityProvider.Facebook,
            Issuer = "https://www.facebook.com",
            Subject = "facebook-subject"
        });

        var resolvedUser = await repository.FindByIdentityAsync(ExternalIdentityProvider.Facebook, "https://www.facebook.com", "facebook-subject");

        Assert.NotNull(resolvedUser);
        Assert.Equal(user.UserId, resolvedUser!.UserId);
        Assert.Equal(2, await context.UserIdentities!.CountAsync(identity => identity.UserId == user.UserId));
    }

    [Theory]
    [InlineData("", "subject")]
    [InlineData("issuer", "")]
    [InlineData(" ", "subject")]
    [InlineData("issuer", " ")]
    public async Task FindByIdentityAsync_WhenIdentityTupleIsIncomplete_Throws(string issuer, string subject)
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repository.FindByIdentityAsync(ExternalIdentityProvider.Google, issuer, subject));
    }

    private static GetTogetherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GetTogetherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GetTogetherDbContext(options);
    }
}