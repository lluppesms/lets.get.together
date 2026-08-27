using GetTogether.Data.Models;

namespace GetTogether.Data.Services;

/// <summary>
/// Coordinates secure onboarding, provider identities, and verified email aliases.
/// </summary>
public interface IAccountIdentityService
{
    /// <summary>
    /// Creates an email-bound invitation for an active circle member.
    /// </summary>
    Task<InvitationCode> CreateInvitationAsync(int circleId, int createdByUserId, string recipientEmailAddress, DateTime? expiresUtc = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a mailbox verification challenge for an email-bound invitation.
    /// </summary>
    Task<EmailVerificationChallenge> BeginInvitationVerificationAsync(string invitationCode, string recipientEmailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically creates an account and circle membership after verified invitation mailbox control.
    /// </summary>
    Task<User> CompleteOnboardingAsync(string invitationCode, string recipientEmailAddress, string verificationToken, string displayName, ExternalIdentityInput identity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a distinct external provider identity after recent authentication.
    /// </summary>
    Task<UserIdentity> LinkIdentityAsync(int userId, ExternalIdentityInput identity, bool recentlyAuthenticated, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks an external provider identity after recent authentication unless it is the user's last identity.
    /// </summary>
    Task UnlinkIdentityAsync(int userId, int userIdentityId, bool recentlyAuthenticated, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a pending alias and issues its verification token.
    /// </summary>
    Task<EmailVerificationChallenge> AddEmailAliasAsync(int userId, string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a pending alias using a single-use token.
    /// </summary>
    Task VerifyEmailAliasAsync(int userId, int userEmailAliasId, string verificationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects a verified alias as the user's designated alias.
    /// </summary>
    Task DesignateEmailAliasAsync(int userId, int userEmailAliasId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an alias owned by the user.
    /// </summary>
    Task RemoveEmailAliasAsync(int userId, int userEmailAliasId, CancellationToken cancellationToken = default);
}