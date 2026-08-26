//-----------------------------------------------------------------------
// <copyright file="INotificationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Notification Service Interface
// </summary>
//-----------------------------------------------------------------------
#nullable enable
using GetTogether.Data.Models;

namespace GetTogether.Web.Services.Interfaces;

/// <summary>
/// Sends event-related email notifications to circle members via SendGrid.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a creation notification email to all members of the event's circle.
    /// Called immediately after a new event is created.
    /// </summary>
    Task SendEventCreatedAsync(Event newEvent, IEnumerable<User> recipients);

    /// <summary>
    /// Sends a creation notification email to all members of the event's circle.
    /// </summary>
    Task SendEventCreationEmailAsync(Event newEvent, IEnumerable<User> recipients);

    /// <summary>
    /// Sends a reminder email to the specified recipients for the given event.
    /// Called manually by any circle member (per OQ-1).
    /// </summary>
    Task SendReminderAsync(Event evt, IEnumerable<User> recipients);

    /// <summary>
    /// Sends a reminder email to the specified recipients for the given event, validating active circle membership for the trigger user and target recipients.
    /// </summary>
    Task SendReminderEmailAsync(Event evt, IEnumerable<User> recipients, int? triggeringUserId = null);
}
