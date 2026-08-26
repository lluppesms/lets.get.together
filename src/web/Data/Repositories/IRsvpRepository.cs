//-----------------------------------------------------------------------
// <copyright file="IRsvpRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// RSVP Repository Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the repository interface for accessing and managing RSVP data.
/// </summary>
public interface IRsvpRepository
{
    /// <summary>
    /// Returns the current RSVP for a user on an event, or null if none exists.
    /// </summary>
    Task<RSVP?> GetRsvpAsync(int eventId, int userId);

    /// <summary>
    /// Returns all RSVPs for the specified event. Caller must be a circle member.
    /// </summary>
    Task<IList<RSVP>> GetRsvpsForEventAsync(int eventId, int requestingUserId);

    /// <summary>
    /// Returns all RSVPs for the specified event. Caller must be an active circle member.
    /// </summary>
    Task<IList<RSVP>> GetRsvpsByEventAsync(int eventId, int requestingUserId);

    /// <summary>
    /// Returns all RSVPs for the specified event occurrence date. Caller must be an active circle member.
    /// </summary>
    Task<IList<RSVP>> GetRsvpsByOccurrenceAsync(int eventId, DateTime occurrenceDate, int requestingUserId);

    /// <summary>
    /// Creates or updates the RSVP for the specified user on the specified event.
    /// Allowed status values: Accept, Decline, Maybe.
    /// Returns the persisted RSVP.
    /// </summary>
    Task<RSVP> UpsertRsvpAsync(int eventId, int userId, string status, string? notes = null, DateTime? occurrenceDate = null);

    /// <summary>
    /// Returns all active circle members who have not yet RSVP'd to the specified event.
    /// Enforces circle membership guard for the requesting user.
    /// </summary>
    Task<IList<User>> GetUnansweredMembersAsync(int eventId, int requestingUserId, DateTime? occurrenceDate = null);
}
