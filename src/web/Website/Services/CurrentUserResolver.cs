using System.Security.Claims;
using GetTogether.Data.Models;
using GetTogether.Data.Repositories;

namespace GetTogether.Web.Services;

/// <summary>
/// Resolves the authenticated external identity to a persisted application user.
/// </summary>
public interface ICurrentUserResolver
{
    /// <summary>
    /// Resolves the current application user from a validated authentication scheme and claims contract.
    /// </summary>
    Task<CurrentUserResolution> ResolveAsync(ClaimsPrincipal principal);
}

/// <summary>
/// Represents the outcome of resolving an authenticated principal to an application user.
/// </summary>
public sealed record CurrentUserResolution(User? User, string? FailureReason)
{
    /// <summary>
    /// Gets whether the authenticated identity was recognized and contained the required claims.
    /// </summary>
    public bool HasRecognizedIdentity => FailureReason is null;
}

/// <summary>
/// Resolves provider-qualified external identities without using mutable profile claims as identity keys.
/// </summary>
public sealed class CurrentUserResolver(IUserRepository userRepository, ILogger<CurrentUserResolver>? logger = null) : ICurrentUserResolver
{
    /// <inheritdoc />
    public async Task<CurrentUserResolution> ResolveAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var identity = principal.Identity;
        var authenticatedUserName = principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.Identity?.Name
            ?? principal.FindFirstValue("name")
            ?? principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? "(name unavailable)";
        if (identity?.IsAuthenticated != true)
        {
            logger?.LogWarning("Current user resolution failed for {AuthenticatedUserName}: principal is not authenticated. AuthenticationType={AuthenticationType}", authenticatedUserName, identity?.AuthenticationType);
            return new CurrentUserResolution(null, "The current request is not authenticated.");
        }

        if (!TryGetProvider(principal, out var provider))
        {
            logger?.LogWarning("Current user resolution failed for {AuthenticatedUserName}: provider claim is missing or invalid. AuthenticationType={AuthenticationType} ClaimTypes={ClaimTypes}", authenticatedUserName, identity.AuthenticationType, string.Join(",", principal.Claims.Select(claim => claim.Type).Distinct()));
            return new CurrentUserResolution(null, "The authenticated provider is not recognized.");
        }

        var subject = principal.FindFirstValue(ExternalIdentityClaimTypes.Subject)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var issuer = principal.FindFirstValue(ExternalIdentityClaimTypes.Issuer)
            ?? principal.FindFirstValue("iss");
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(issuer))
        {
            logger?.LogWarning("Current user resolution failed for {AuthenticatedUserName}: required identity values are missing. Provider={Provider} HasSubject={HasSubject} HasIssuer={HasIssuer} ClaimTypes={ClaimTypes}", authenticatedUserName, provider, !string.IsNullOrWhiteSpace(subject), !string.IsNullOrWhiteSpace(issuer), string.Join(",", principal.Claims.Select(claim => claim.Type).Distinct()));
            return new CurrentUserResolution(null, "The authenticated provider did not supply a valid identity subject and issuer.");
        }

        var user = await userRepository.FindByIdentityAsync(provider, issuer, subject);
        logger?.LogInformation("Current user identity lookup completed. Provider={Provider} Issuer={Issuer} UserFound={UserFound}", provider, issuer, user is not null);
        return new CurrentUserResolution(user, null);
    }

    private static bool TryGetProvider(ClaimsPrincipal principal, out ExternalIdentityProvider provider)
    {
        return Enum.TryParse(principal.FindFirstValue(ExternalIdentityClaimTypes.Provider), ignoreCase: false, out provider)
            && Enum.IsDefined(provider);
    }
}

/// <summary>
/// Provides an explicit resolution result when the SQL-backed user repository is unavailable.
/// </summary>
public sealed class UnavailableCurrentUserResolver : ICurrentUserResolver
{
    /// <inheritdoc />
    public Task<CurrentUserResolution> ResolveAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return Task.FromResult(new CurrentUserResolution(null, "Application account resolution is unavailable because SQL data is not configured."));
    }
}