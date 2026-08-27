using GetTogether.Data.Models;

namespace GetTogether.Data.Services;

/// <summary>
/// Identifies a validated external provider identity.
/// </summary>
public sealed record ExternalIdentityInput(ExternalIdentityProvider Provider, string Issuer, string Subject);

/// <summary>
/// Describes a mailbox verification challenge without exposing its secret token.
/// </summary>
public sealed record EmailVerificationChallenge(string EmailAddress, DateTime ExpiresUtc);