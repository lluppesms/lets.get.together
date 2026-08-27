using System.Security.Claims;
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;
using GetTogether.Web.Services;
using Moq;

namespace GetTogether.Tests;

public class CurrentUserResolver_Tests
{
    [Fact]
    public async Task ResolveAsync_UsesSchemeQualifiedProviderForGoogleAndFacebook()
    {
        var googleUser = new User { UserId = 101, DisplayName = "Google User" };
        var facebookUser = new User { UserId = 202, DisplayName = "Facebook User" };
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        repository
            .Setup(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Google, "https://accounts.google.com", "google-subject"))
            .ReturnsAsync(googleUser);
        repository
            .Setup(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Facebook, "https://www.facebook.com", "facebook-subject"))
            .ReturnsAsync(facebookUser);
        var resolver = new CurrentUserResolver(repository.Object);

        var googleResolution = await resolver.ResolveAsync(CreatePrincipal("Google", "https://accounts.google.com", "google-subject"));
        var facebookResolution = await resolver.ResolveAsync(CreatePrincipal("Facebook", "https://www.facebook.com", "facebook-subject"));

        Assert.Equal(googleUser.UserId, googleResolution.User?.UserId);
        Assert.Equal(facebookUser.UserId, facebookResolution.User?.UserId);
        repository.Verify(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Google, "https://accounts.google.com", "google-subject"), Times.Once);
        repository.Verify(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Facebook, "https://www.facebook.com", "facebook-subject"), Times.Once);
        repository.Verify(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Entra, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_UsesSchemeQualifiedProviderForEntra()
    {
        var entraUser = new User { UserId = 303, DisplayName = "Entra User" };
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        repository
            .Setup(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Entra, "https://login.microsoftonline.com/tenant-id/v2.0", "entra-subject"))
            .ReturnsAsync(entraUser);
        var resolver = new CurrentUserResolver(repository.Object);

        var resolution = await resolver.ResolveAsync(CreatePrincipal("OpenIdConnect", "https://login.microsoftonline.com/tenant-id/v2.0", "entra-subject"));

        Assert.Equal(entraUser.UserId, resolution.User?.UserId);
        repository.Verify(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Entra, "https://login.microsoftonline.com/tenant-id/v2.0", "entra-subject"), Times.Once);
    }

    [Theory]
    [InlineData(ExternalIdentityProvider.Entra, "https://login.microsoftonline.com/tenant-id/v2.0", "entra-subject")]
    [InlineData(ExternalIdentityProvider.Google, "https://accounts.google.com", "google-subject")]
    [InlineData(ExternalIdentityProvider.Facebook, "https://www.facebook.com", "facebook-subject")]
    public async Task ResolveAsync_UsesProviderClaimForCookieAuthenticatedIdentity(ExternalIdentityProvider provider, string issuer, string subject)
    {
        var user = new User { UserId = 404, DisplayName = "Cookie User" };
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        repository
            .Setup(repository => repository.FindByIdentityAsync(provider, issuer, subject))
            .ReturnsAsync(user);
        var resolver = new CurrentUserResolver(repository.Object);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim("iss", issuer),
            new Claim(ExternalIdentityClaimTypes.Provider, provider.ToString())
        ], "Cookies"));

        var resolution = await resolver.ResolveAsync(principal);

        Assert.Equal(user.UserId, resolution.User?.UserId);
        repository.Verify(repository => repository.FindByIdentityAsync(provider, issuer, subject), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WhenAuthenticationSchemeIsUnknown_ReturnsExplicitFailureWithoutRepositoryLookup()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var resolver = new CurrentUserResolver(repository.Object);

        var resolution = await resolver.ResolveAsync(CreatePrincipal("Untrusted", "https://issuer.example.test", "subject"));

        Assert.Null(resolution.User);
        Assert.Equal("The authenticated provider is not recognized.", resolution.FailureReason);
    }

    private static ClaimsPrincipal CreatePrincipal(string authenticationType, string issuer, string subject)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim("iss", issuer)
        ],
        authenticationType);

        return new ClaimsPrincipal(identity);
    }
}