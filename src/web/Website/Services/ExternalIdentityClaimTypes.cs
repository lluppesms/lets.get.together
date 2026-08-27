namespace GetTogether.Web.Services;

/// <summary>
/// Defines claims issued by Get Together to preserve validated external identity context.
/// </summary>
public static class ExternalIdentityClaimTypes
{
    /// <summary>
    /// Gets the claim type containing the provider that validated the external identity.
    /// </summary>
    public const string Provider = "get-together:external-identity-provider";
}