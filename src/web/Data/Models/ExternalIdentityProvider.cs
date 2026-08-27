namespace GetTogether.Data.Models;

/// <summary>
/// Identifies an external authentication provider supported by Get Together.
/// </summary>
public enum ExternalIdentityProvider
{
    /// <summary>
    /// Microsoft Entra ID.
    /// </summary>
    Entra = 1,

    /// <summary>
    /// Google.
    /// </summary>
    Google = 2,

    /// <summary>
    /// Facebook.
    /// </summary>
    Facebook = 3
}