using System.Security.Claims;
using GetTogether.Data.Models;

namespace GetTogether.Web.Services;

/// <summary>
/// Preserves validated external identity context in the application authentication ticket.
/// </summary>
public static class ExternalIdentityClaims
{
    /// <summary>
    /// Adds the external provider marker, issuer, and normalized subject required to resolve an application identity.
    /// </summary>
    public static void EnsureClaims(ClaimsIdentity? identity, ExternalIdentityProvider provider, string? issuer)
    {
        if (identity is null)
        {
            return;
        }

        var validatedIssuer = identity.FindFirst("iss")?.Value ?? issuer;
        if (!string.IsNullOrWhiteSpace(validatedIssuer))
        {
            if (identity.FindFirst("iss") is null)
            {
                identity.AddClaim(new Claim("iss", validatedIssuer));
            }

            if (identity.FindFirst(ExternalIdentityClaimTypes.Issuer) is null)
            {
                identity.AddClaim(new Claim(ExternalIdentityClaimTypes.Issuer, validatedIssuer));
            }
        }

        var subject = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? identity.FindFirst("sub")?.Value
            ?? identity.FindFirst("oid")?.Value
            ?? identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (!string.IsNullOrWhiteSpace(subject))
        {
            if (identity.FindFirst(ClaimTypes.NameIdentifier) is null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subject));
            }

            if (identity.FindFirst(ExternalIdentityClaimTypes.Subject) is null)
            {
                identity.AddClaim(new Claim(ExternalIdentityClaimTypes.Subject, subject));
            }
        }

        if (identity.FindFirst(ExternalIdentityClaimTypes.Provider) is null)
        {
            identity.AddClaim(new Claim(ExternalIdentityClaimTypes.Provider, provider.ToString()));
        }
    }
}