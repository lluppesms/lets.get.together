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
public sealed class CurrentUserResolver(IUserRepository userRepository) : ICurrentUserResolver
{
    /// <inheritdoc />
    public async Task<CurrentUserResolution> ResolveAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true)
        {
            return new CurrentUserResolution(null, "The current request is not authenticated.");
        }

        if (!TryGetProvider(principal, out var provider))
        {
            return new CurrentUserResolution(null, "The authenticated provider is not recognized.");
        }

        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var issuer = principal.FindFirstValue("iss");
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(issuer))
        {
            return new CurrentUserResolution(null, "The authenticated provider did not supply a valid identity subject and issuer.");
        }

        var user = await userRepository.FindByIdentityAsync(provider, issuer, subject);
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