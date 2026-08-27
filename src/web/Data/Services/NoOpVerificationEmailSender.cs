namespace GetTogether.Data.Services;

/// <summary>
/// Development sender that deliberately does not deliver email.
/// </summary>
public sealed class NoOpVerificationEmailSender : IVerificationEmailSender
{
    /// <inheritdoc/>
    public Task SendAsync(string emailAddress, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Task.CompletedTask;
    }
}