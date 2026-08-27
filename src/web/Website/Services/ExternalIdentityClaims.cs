using System.Security.Claims;
using GetTogether.Data.Models;

namespace GetTogether.Web.Services;

/// <summary>
/// Preserves validated external identity context in the application authentication ticket.
/// </summary>
public static class ExternalIdentityClaims
{
    /// <summary>
    /// Adds the external provider marker and issuer required to resolve an application identity.
    /// </summary>
    public static void EnsureClaims(ClaimsIdentity? identity, ExternalIdentityProvider provider, string? issuer)
    {
        if (identity is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(issuer) && identity.FindFirst("iss") is null)
        {
            identity.AddClaim(new Claim("iss", issuer));
        }

        if (identity.FindFirst(ExternalIdentityClaimTypes.Provider) is null)
        {
            identity.AddClaim(new Claim(ExternalIdentityClaimTypes.Provider, provider.ToString()));
        }
    }
}