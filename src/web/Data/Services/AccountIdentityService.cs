using System.Net.Mail;
using System.Security.Cryptography;
using GetTogether.Data.Models;

namespace GetTogether.Data.Services;

/// <summary>
/// Coordinates email-proven onboarding and account identity operations.
/// </summary>
public sealed class AccountIdentityService(
    GetTogetherDbContext context,
    IVerificationEmailSender verificationEmailSender) : IAccountIdentityService
{
    private static readonly TimeSpan VerificationLifetime = TimeSpan.FromMinutes(15);
    private readonly GetTogetherDbContext _context = context;
    private readonly IVerificationEmailSender _verificationEmailSender = verificationEmailSender;

    /// <inheritdoc/>
    public async Task<InvitationCode> CreateInvitationAsync(int circleId, int createdByUserId, string recipientEmailAddress, DateTime? expiresUtc = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmailAddress = NormalizeEmailAddress(recipientEmailAddress);
        var isMember = await _context.CircleMemberships!
            .AnyAsync(membership => membership.CircleId == circleId && membership.UserId == createdByUserId && membership.LeftUtc == null, cancellationToken);
        if (!isMember)
        {
            throw new InvalidOperationException("Only active circle members may create invitations.");
        }

        var invitation = new InvitationCode
        {
            CircleId = circleId,
            CreatedByUserId = createdByUserId,
            Code = Guid.NewGuid().ToString("N"),
            RecipientEmailAddress = recipientEmailAddress.Trim(),
            NormalizedRecipientEmailAddress = normalizedEmailAddress,
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = expiresUtc
        };

        _context.InvitationCodes!.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    /// <inheritdoc/>
    public async Task<EmailVerificationChallenge> BeginInvitationVerificationAsync(string invitationCode, string recipientEmailAddress, CancellationToken cancellationToken = default)
    {
        var normalizedEmailAddress = NormalizeEmailAddress(recipientEmailAddress);
        var invitation = await FindValidInvitationAsync(invitationCode, cancellationToken);
        if (invitation is null || invitation.NormalizedRecipientEmailAddress != normalizedEmailAddress)
        {
            throw new InvalidOperationException("Unable to verify this invitation.");
        }

        return await CreateAndSendTokenAsync(invitation.RecipientEmailAddress!, normalizedEmailAddress, invitation.InvitationCodeId, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<User> CompleteOnboardingAsync(string invitationCode, string recipientEmailAddress, string verificationToken, string displayName, ExternalIdentityInput identity, CancellationToken cancellationToken = default)
    {
        var normalizedEmailAddress = NormalizeEmailAddress(recipientEmailAddress);
        ValidateDisplayName(displayName);
        ValidateIdentity(identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationToken);

        await using var transaction = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(cancellationToken)
            : null;

        var now = DateTime.UtcNow;
        var invitation = await _context.InvitationCodes!
            .SingleOrDefaultAsync(candidate => candidate.Code == invitationCode && candidate.RedeemedUtc == null && candidate.RevokedUtc == null &&
                (candidate.ExpiresUtc == null || candidate.ExpiresUtc > now) && candidate.NormalizedRecipientEmailAddress == normalizedEmailAddress, cancellationToken);
        var token = invitation is null ? null : await _context.EmailVerificationTokens!
            .SingleOrDefaultAsync(candidate => candidate.InvitationCodeId == invitation.InvitationCodeId && candidate.NormalizedEmailAddress == normalizedEmailAddress &&
                candidate.TokenHash == HashToken(verificationToken) && candidate.UsedUtc == null && candidate.ExpiresUtc > now, cancellationToken);
        var identityExists = await _context.UserIdentities!
            .AnyAsync(candidate => candidate.Provider == identity.Provider && candidate.Issuer == identity.Issuer && candidate.Subject == identity.Subject, cancellationToken);

        if (invitation is null || token is null || identityExists)
        {
            throw new InvalidOperationException("Unable to complete account setup.");
        }

        var user = new User { DisplayName = displayName.Trim(), IsActive = true, CreatedUtc = now };
        var userIdentity = new UserIdentity { Provider = identity.Provider, Issuer = identity.Issuer, Subject = identity.Subject, CreatedUtc = now };
        var emailAlias = new UserEmailAlias
        {
            EmailAddress = recipientEmailAddress.Trim(),
            NormalizedEmailAddress = normalizedEmailAddress,
            IsVerified = true,
            IsPrimary = true,
            VerifiedUtc = now,
            CreatedUtc = now
        };
        user.Identities.Add(userIdentity);
        user.EmailAliases.Add(emailAlias);
        invitation.RedeemedByUser = user;
        invitation.RedeemedUtc = now;
        token.UsedUtc = now;
        _context.Users!.Add(user);
        _context.CircleMemberships!.Add(new CircleMembership { CircleId = invitation.CircleId, User = user, Role = "Member", JoinedUtc = now });

        await _context.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return user;
    }

    /// <inheritdoc/>
    public async Task<UserIdentity> LinkIdentityAsync(int userId, ExternalIdentityInput identity, bool recentlyAuthenticated, CancellationToken cancellationToken = default)
    {
        RequireRecentAuthentication(recentlyAuthenticated);
        ValidateIdentity(identity);
        var userExists = await _context.Users!.AnyAsync(user => user.UserId == userId && user.IsActive, cancellationToken);
        var identityExists = await _context.UserIdentities!.AnyAsync(candidate => candidate.Provider == identity.Provider && candidate.Issuer == identity.Issuer && candidate.Subject == identity.Subject, cancellationToken);
        if (!userExists || identityExists)
        {
            throw new InvalidOperationException("Unable to link this identity.");
        }

        var userIdentity = new UserIdentity { UserId = userId, Provider = identity.Provider, Issuer = identity.Issuer, Subject = identity.Subject, CreatedUtc = DateTime.UtcNow };
        _context.UserIdentities!.Add(userIdentity);
        await _context.SaveChangesAsync(cancellationToken);
        return userIdentity;
    }

    /// <inheritdoc/>
    public async Task UnlinkIdentityAsync(int userId, int userIdentityId, bool recentlyAuthenticated, CancellationToken cancellationToken = default)
    {
        RequireRecentAuthentication(recentlyAuthenticated);
        var identity = await _context.UserIdentities!.SingleOrDefaultAsync(candidate => candidate.UserIdentityId == userIdentityId && candidate.UserId == userId, cancellationToken);
        var identityCount = await _context.UserIdentities!.CountAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (identity is null || identityCount <= 1)
        {
            throw new InvalidOperationException("Unable to unlink this identity.");
        }

        _context.UserIdentities!.Remove(identity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EmailVerificationChallenge> AddEmailAliasAsync(int userId, string emailAddress, CancellationToken cancellationToken = default)
    {
        var normalizedEmailAddress = NormalizeEmailAddress(emailAddress);
        var userExists = await _context.Users!.AnyAsync(user => user.UserId == userId && user.IsActive, cancellationToken);
        var aliasExists = await _context.UserEmailAliases!.AnyAsync(alias => alias.NormalizedEmailAddress == normalizedEmailAddress, cancellationToken);
        if (!userExists || aliasExists)
        {
            throw new InvalidOperationException("Unable to add this email address.");
        }

        var alias = new UserEmailAlias { UserId = userId, EmailAddress = emailAddress.Trim(), NormalizedEmailAddress = normalizedEmailAddress, CreatedUtc = DateTime.UtcNow };
        _context.UserEmailAliases!.Add(alias);
        await _context.SaveChangesAsync(cancellationToken);
        return await CreateAndSendTokenAsync(alias.EmailAddress, normalizedEmailAddress, null, alias.UserEmailAliasId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task VerifyEmailAliasAsync(int userId, int userEmailAliasId, string verificationToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationToken);
        var now = DateTime.UtcNow;
        var alias = await _context.UserEmailAliases!.SingleOrDefaultAsync(candidate => candidate.UserEmailAliasId == userEmailAliasId && candidate.UserId == userId && !candidate.IsVerified, cancellationToken);
        var token = alias is null ? null : await _context.EmailVerificationTokens!
            .SingleOrDefaultAsync(candidate => candidate.UserEmailAliasId == userEmailAliasId && candidate.TokenHash == HashToken(verificationToken) && candidate.UsedUtc == null && candidate.ExpiresUtc > now, cancellationToken);
        if (alias is null || token is null)
        {
            throw new InvalidOperationException("Unable to verify this email address.");
        }

        alias.IsVerified = true;
        alias.VerifiedUtc = now;
        token.UsedUtc = now;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DesignateEmailAliasAsync(int userId, int userEmailAliasId, CancellationToken cancellationToken = default)
    {
        var alias = await _context.UserEmailAliases!.SingleOrDefaultAsync(candidate => candidate.UserEmailAliasId == userEmailAliasId && candidate.UserId == userId && candidate.IsVerified, cancellationToken)
            ?? throw new InvalidOperationException("Unable to designate this email address.");
        var aliases = await _context.UserEmailAliases!.Where(candidate => candidate.UserId == userId && candidate.IsPrimary).ToListAsync(cancellationToken);
        foreach (var currentPrimaryAlias in aliases)
        {
            currentPrimaryAlias.IsPrimary = false;
        }

        alias.IsPrimary = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveEmailAliasAsync(int userId, int userEmailAliasId, CancellationToken cancellationToken = default)
    {
        var alias = await _context.UserEmailAliases!.SingleOrDefaultAsync(candidate => candidate.UserEmailAliasId == userEmailAliasId && candidate.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Unable to remove this email address.");
        _context.UserEmailAliases!.Remove(alias);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<InvitationCode?> FindValidInvitationAsync(string invitationCode, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invitationCode);
        var now = DateTime.UtcNow;
        return await _context.InvitationCodes!.SingleOrDefaultAsync(candidate => candidate.Code == invitationCode && candidate.RedeemedUtc == null && candidate.RevokedUtc == null &&
            (candidate.ExpiresUtc == null || candidate.ExpiresUtc > now), cancellationToken);
    }

    private async Task<EmailVerificationChallenge> CreateAndSendTokenAsync(string emailAddress, string normalizedEmailAddress, int? invitationCodeId, int? userEmailAliasId, CancellationToken cancellationToken)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresUtc = DateTime.UtcNow.Add(VerificationLifetime);
        _context.EmailVerificationTokens!.Add(new EmailVerificationToken
        {
            TokenHash = HashToken(token),
            NormalizedEmailAddress = normalizedEmailAddress,
            InvitationCodeId = invitationCodeId,
            UserEmailAliasId = userEmailAliasId,
            ExpiresUtc = expiresUtc,
            CreatedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        await _verificationEmailSender.SendAsync(emailAddress, token, cancellationToken);
        return new EmailVerificationChallenge(emailAddress, expiresUtc);
    }

    private static string NormalizeEmailAddress(string emailAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        try
        {
            return new MailAddress(emailAddress.Trim()).Address.ToUpperInvariant();
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("A valid email address is required.", nameof(emailAddress), exception);
        }
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static void RequireRecentAuthentication(bool recentlyAuthenticated)
    {
        if (!recentlyAuthenticated)
        {
            throw new InvalidOperationException("Recent authentication is required.");
        }
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 200)
        {
            throw new ArgumentException("A display name of 200 characters or fewer is required.", nameof(displayName));
        }
    }

    private static void ValidateIdentity(ExternalIdentityInput identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.Subject);
        if (identity.Issuer.Length > 500 || identity.Subject.Length > 500)
        {
            throw new ArgumentException("The provider identity is invalid.", nameof(identity));
        }
    }
}