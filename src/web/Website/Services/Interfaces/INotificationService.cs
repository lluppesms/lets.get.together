//-----------------------------------------------------------------------
// <copyright file="INotificationService.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Notification Service Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Web.Services.Interfaces;

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
    /// Sends a reminder email to the specified recipients for the given event.
    /// Called manually by any circle member (or only the creator — TBD; see open questions).
    /// </summary>
    Task SendReminderAsync(Event evt, IEnumerable<User> recipients);
}
