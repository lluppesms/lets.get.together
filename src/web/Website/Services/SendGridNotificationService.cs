//-----------------------------------------------------------------------
// <copyright file="SendGridNotificationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// SendGrid-backed notification service
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;
using DadABase.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DadABase.Web.Services;

/// <summary>
/// Sends event notification emails via SendGrid.
/// SendGrid integration details (API key, from address) are read from configuration.
/// </summary>
public class SendGridNotificationService(IConfiguration configuration, ILogger<SendGridNotificationService> logger) : INotificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SendGridNotificationService> _logger = logger;

    /// <inheritdoc/>
    public async Task SendEventCreatedAsync(Event newEvent, IEnumerable<User> recipients)
    {
        // TODO: implement SendGrid delivery.
        // Configuration keys expected: SendGrid:ApiKey, SendGrid:FromEmail, SendGrid:FromName
        _logger.LogInformation(
            "SendEventCreated stub: event {EventId} '{Title}' to {Count} recipient(s).",
            newEvent.EventId, newEvent.Title, recipients.Count());
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SendReminderAsync(Event evt, IEnumerable<User> recipients)
    {
        // TODO: implement SendGrid delivery.
        _logger.LogInformation(
            "SendReminder stub: event {EventId} '{Title}' to {Count} recipient(s).",
            evt.EventId, evt.Title, recipients.Count());
        await Task.CompletedTask;
    }
}
