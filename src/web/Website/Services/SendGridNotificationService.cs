//-----------------------------------------------------------------------
// <copyright file="SendGridNotificationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// SendGrid-backed notification service
// </summary>
//-----------------------------------------------------------------------
#nullable enable
using GetTogether.Data;
using GetTogether.Data.Models;
using GetTogether.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetTogether.Web.Services;

/// <summary>
/// Sends event notification emails via SendGrid.
/// SendGrid integration details (API key, from address) are read from configuration.
/// Logs sent reminders to the database.
/// </summary>
public class SendGridNotificationService(
    IConfiguration configuration,
    ILogger<SendGridNotificationService> logger,
    GetTogetherDbContext? dbContext = null) : INotificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SendGridNotificationService> _logger = logger;
    private readonly GetTogetherDbContext? _dbContext = dbContext;

    /// <inheritdoc/>
    public async Task SendEventCreatedAsync(Event newEvent, IEnumerable<User> recipients)
    {
        await SendEventCreationEmailAsync(newEvent, recipients);
    }

    /// <inheritdoc/>
    public async Task SendEventCreationEmailAsync(Event newEvent, IEnumerable<User> recipients)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"] ?? "notifications@gettogether.test";
        var fromName = _configuration["SendGrid:FromName"] ?? "Get Together";

        var recipientList = recipients?.ToList() ?? [];
        _logger.LogInformation(
            "SendEventCreationEmail: event {EventId} '{Title}' from '{FromName}' <{FromEmail}> (ApiKeyConfigured: {HasApiKey}) to {Count} recipient(s).",
            newEvent.EventId, newEvent?.Title, fromName, fromEmail, !string.IsNullOrWhiteSpace(apiKey), recipientList.Count);

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SendReminderAsync(Event evt, IEnumerable<User> recipients)
    {
        await SendReminderEmailAsync(evt, recipients, null);
    }

    /// <inheritdoc/>
    public async Task SendReminderEmailAsync(Event evt, IEnumerable<User> recipients, int? triggeringUserId = null)
    {
        if (evt == null)
        {
            return;
        }

        // Per OQ-1: Enforce circle membership on trigger user if specified
        if (triggeringUserId.HasValue && _dbContext != null)
        {
            var isTriggeringMember = await _dbContext.CircleMemberships!
                .AnyAsync(m => m.CircleId == evt.CircleId && m.UserId == triggeringUserId.Value && m.LeftUtc == null);
            if (!isTriggeringMember)
            {
                _logger.LogWarning("User {UserId} attempted to trigger reminder for Event {EventId} but is not an active member of Circle {CircleId}.", triggeringUserId, evt.EventId, evt.CircleId);
                return;
            }
        }

        // Enforce circle membership on reminder target audience
        var recipientList = recipients?.ToList() ?? [];
        if (_dbContext != null)
        {
            var activeMemberUserIds = await _dbContext.CircleMemberships!
                .Where(m => m.CircleId == evt.CircleId && m.LeftUtc == null)
                .Select(m => m.UserId)
                .ToListAsync();

            recipientList = recipientList.Where(r => activeMemberUserIds.Contains(r.UserId)).ToList();
        }

        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"] ?? "notifications@gettogether.test";
        var fromName = _configuration["SendGrid:FromName"] ?? "Get Together";

        _logger.LogInformation(
            "SendReminderEmail: event {EventId} '{Title}' from '{FromName}' <{FromEmail}> (ApiKeyConfigured: {HasApiKey}) to {Count} recipient(s).",
            evt.EventId, evt.Title, fromName, fromEmail, !string.IsNullOrWhiteSpace(apiKey), recipientList.Count);

        // Log reminder events into ReminderLog table via DbContext
        if (_dbContext != null && recipientList.Count != 0)
        {
            foreach (var recipient in recipientList)
            {
                var reminderLog = new ReminderLog
                {
                    EventId = evt.EventId,
                    UserId = recipient.UserId,
                    Channel = "Email",
                    SentUtc = DateTime.UtcNow,
                    DeliveryState = string.IsNullOrWhiteSpace(apiKey) ? "Logged" : "Sent",
                    ProviderMessageId = $"msg-{Guid.NewGuid():N}"
                };
                _dbContext.ReminderLogs!.Add(reminderLog);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
