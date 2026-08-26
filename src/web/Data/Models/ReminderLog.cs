//-----------------------------------------------------------------------
// <copyright file="ReminderLog.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// ReminderLog Table
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Data.Models;

/// <summary>
/// Represents a sent reminder entry for event notifications.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("ReminderLog", Schema = "Meetings")]
public class ReminderLog
{
    /// <summary>
    /// Gets or sets the unique identifier for the reminder log record.
    /// </summary>
    [Key, Column(Order = 0)]
    public int ReminderLogId { get; set; }

    /// <summary>
    /// Gets or sets the associated event identifier.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the associated user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the delivery channel used for this reminder.
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Channel { get; set; } = "InApp";

    /// <summary>
    /// Gets or sets the UTC date and time when the reminder was sent.
    /// </summary>
    public DateTime SentUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the delivery state for this reminder.
    /// </summary>
    [Required]
    [StringLength(30)]
    public string DeliveryState { get; set; } = "Queued";

    /// <summary>
    /// Gets an optional provider message identifier for traceability.
    /// </summary>
    [StringLength(120)]
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Gets the event associated with this reminder.
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// Gets the user associated with this reminder.
    /// </summary>
    public User? User { get; set; }
}
