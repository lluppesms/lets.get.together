using System.Net.Http.Headers;
using System.Net.Http.Json;
using GetTogether.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetTogether.Web.Services;

/// <summary>
/// Delivers mailbox verification tokens through the configured SendGrid account.
/// </summary>
public sealed class SendGridVerificationEmailSender(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<SendGridVerificationEmailSender> logger) : IVerificationEmailSender
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["SendGrid:ApiKey"]!;
    private readonly string _fromEmail = configuration["SendGrid:FromEmail"]!;
    private readonly string _fromName = configuration["SendGrid:FromName"] ?? "Get Together";
    private readonly ILogger<SendGridVerificationEmailSender> _logger = logger;

    /// <inheritdoc/>
    public async Task SendAsync(string emailAddress, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
        {
            Content = JsonContent.Create(new
            {
                personalizations = new[] { new { to = new[] { new { email = emailAddress } } } },
                from = new { email = _fromEmail, name = _fromName },
                subject = "Verify your Get Together email address",
                content = new[] { new { type = "text/plain", value = $"Your Get Together verification code is: {token}" } }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("SendGrid rejected the verification email for {EmailAddress} with status code {StatusCode}.", emailAddress, response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
    }
}