using System.Security.Claims;
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;
using GetTogether.Web.Services;
using Moq;

namespace GetTogether.Tests;

public class CurrentUserResolverGuard_Tests
{
    [Fact]
    public async Task ResolveAsync_WhenPrincipalIsUnauthenticated_ReturnsFailureWithoutRepositoryLookup()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var resolver = new CurrentUserResolver(repository.Object);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var resolution = await resolver.ResolveAsync(principal);

        Assert.Null(resolution.User);
        Assert.Equal("The current request is not authenticated.", resolution.FailureReason);
    }

    [Theory]
    [InlineData(null, "subject")]
    [InlineData("https://issuer.example.test", null)]
    [InlineData(" ", "subject")]
    [InlineData("https://issuer.example.test", " ")]
    public async Task ResolveAsync_WhenIssuerOrSubjectIsMissing_ReturnsFailureWithoutRepositoryLookup(string? issuer, string? subject)
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var resolver = new CurrentUserResolver(repository.Object);
        var claims = new List<Claim>();
        if (issuer is not null)
        {
            claims.Add(new Claim("iss", issuer));
        }

        if (subject is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));

        var resolution = await resolver.ResolveAsync(principal);

        Assert.Null(resolution.User);
        Assert.Equal("The authenticated provider did not supply a valid identity subject and issuer.", resolution.FailureReason);
    }

    [Fact]
    public async Task ResolveAsync_DoesNotUseEmailClaimToResolveAnIdentity()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        repository
            .Setup(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Google, "https://accounts.google.com", "stable-subject"))
            .ReturnsAsync((User?)null);
        var resolver = new CurrentUserResolver(repository.Object);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "stable-subject"),
            new Claim("iss", "https://accounts.google.com"),
            new Claim(ClaimTypes.Email, "changed-address@example.test")
        ], "Google"));

        var resolution = await resolver.ResolveAsync(principal);

        Assert.Null(resolution.User);
        Assert.Null(resolution.FailureReason);
        repository.Verify(repository => repository.FindByIdentityAsync(ExternalIdentityProvider.Google, "https://accounts.google.com", "stable-subject"), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WhenCookieProviderClaimIsMissing_ReturnsFailureWithoutRepositoryLookup()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var resolver = new CurrentUserResolver(repository.Object);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "subject"),
            new Claim("iss", "https://issuer.example.test")
        ], "Cookies"));

        var resolution = await resolver.ResolveAsync(principal);

        Assert.Null(resolution.User);
        Assert.Equal("The authenticated provider is not recognized.", resolution.FailureReason);
    }
}