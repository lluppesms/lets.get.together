using System.Security.Claims;
using GetTogether.Data.Models;
using GetTogether.Web.Services;

namespace GetTogether.Tests;

public class ExternalIdentityClaims_Tests
{
    [Fact]
    public void EnsureClaims_WhenGoogleTicketHasSubject_AddsResolverClaims()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "google-subject")], "Google");

        ExternalIdentityClaims.EnsureClaims(identity, ExternalIdentityProvider.Google, "https://accounts.google.com");

        Assert.Equal("google-subject", identity.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("https://accounts.google.com", identity.FindFirst("iss")?.Value);
        Assert.Equal(ExternalIdentityProvider.Google.ToString(), identity.FindFirst(ExternalIdentityClaimTypes.Provider)?.Value);
    }

    [Fact]
    public void EnsureClaims_WhenGoogleTicketAlreadyHasIssuer_PreservesValidatedIssuer()
    {
        var identity = new ClaimsIdentity([new Claim("iss", "https://validated-google-issuer.example.test")], "Google");

        ExternalIdentityClaims.EnsureClaims(identity, ExternalIdentityProvider.Google, "https://accounts.google.com");

        Assert.Equal("https://validated-google-issuer.example.test", identity.FindFirst("iss")?.Value);
    }
}