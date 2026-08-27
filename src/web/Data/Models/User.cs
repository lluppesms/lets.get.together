//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// User Table
// </summary>
//-----------------------------------------------------------------------
namespace GetTogether.Data.Models;

/// <summary>
/// Represents an application user in the Get Together domain.
/// </summary>
[ExcludeFromCodeCoverage]
[Table("User", Schema = "Meetings")]
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key, Column(Order = 0)]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the display name for the user.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC date and time when the user was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the first verified email alias available for display.
    /// </summary>
    [NotMapped]
    public string? PrimaryVerifiedEmailAddress => EmailAliases
        .Where(alias => alias.IsVerified)
        .OrderByDescending(alias => alias.IsPrimary)
        .ThenBy(alias => alias.CreatedUtc)
        .Select(alias => alias.EmailAddress)
        .FirstOrDefault();

    /// <summary>
    /// Gets external identities linked to this user.
    /// </summary>
    public ICollection<UserIdentity> Identities { get; set; } = [];

    /// <summary>
    /// Gets verified and pending email aliases linked to this user.
    /// </summary>
    public ICollection<UserEmailAlias> EmailAliases { get; set; } = [];

    /// <summary>
    /// Gets memberships for circles that this user belongs to.
    /// </summary>
    public ICollection<CircleMembership> CircleMemberships { get; set; } = [];

    /// <summary>
    /// Gets invitation codes created by this user.
    /// </summary>
    public ICollection<InvitationCode> InvitationCodesCreated { get; set; } = [];

    /// <summary>
    /// Gets invitation codes redeemed by this user.
    /// </summary>
    public ICollection<InvitationCode> InvitationCodesRedeemed { get; set; } = [];

    /// <summary>
    /// Gets circles created by this user.
    /// </summary>
    public ICollection<Circle> CirclesCreated { get; set; } = [];

    /// <summary>
    /// Gets events created by this user.
    /// </summary>
    public ICollection<Event> EventsCreated { get; set; } = [];

    /// <summary>
    /// Gets RSVPs for this user.
    /// </summary>
    public ICollection<RSVP> Rsvps { get; set; } = [];

    /// <summary>
    /// Gets reminder logs for this user.
    /// </summary>
    public ICollection<ReminderLog> ReminderLogs { get; set; } = [];
}
