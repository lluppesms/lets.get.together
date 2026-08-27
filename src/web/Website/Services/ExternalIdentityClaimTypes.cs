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

    /// <summary>
    /// Gets the claim type containing the validated external issuer.
    /// </summary>
    public const string Issuer = "get-together:external-identity-issuer";

    /// <summary>
    /// Gets the claim type containing the validated external subject.
    /// </summary>
    public const string Subject = "get-together:external-identity-subject";
}