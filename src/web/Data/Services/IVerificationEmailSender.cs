namespace GetTogether.Data.Services;

/// <summary>
/// Delivers a mailbox verification token without exposing provider credentials to domain services.
/// </summary>
public interface IVerificationEmailSender
{
    /// <summary>
    /// Sends a verification token to a mailbox.
    /// </summary>
    Task SendAsync(string emailAddress, string token, CancellationToken cancellationToken = default);
}